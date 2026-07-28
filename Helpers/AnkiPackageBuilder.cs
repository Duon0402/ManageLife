using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public static class AnkiPackageBuilder
    {
        private const string CreateSchemaSql = @"
            CREATE TABLE col (id integer primary key, crt integer not null, mod integer not null, scm integer not null, ver integer not null, dty integer not null, usn integer not null, ls integer not null, conf text not null, models text not null, decks text not null, dconf text not null, tags text not null);
            CREATE TABLE notes (id integer primary key, guid text not null, mid integer not null, mod integer not null, usn integer not null, tags text not null, flds text not null, sfld text not null, csum integer not null, flags integer not null, data text not null);
            CREATE TABLE cards (id integer primary key, nid integer not null, did integer not null, ord integer not null, mod integer not null, usn integer not null, type integer not null, queue integer not null, due integer not null, ivl integer not null, factor integer not null, reps integer not null, lapses integer not null, left integer not null, odue integer not null, odid integer not null, flags integer not null, data text not null);
            CREATE TABLE revlog (id integer primary key, cid integer not null, usn integer not null, ease integer not null, ivl integer not null, lastIvl integer not null, factor integer not null, time integer not null, type integer not null);
            CREATE TABLE graves (usn integer not null, oid integer not null, type integer not null);
            CREATE INDEX ix_notes_usn on notes (usn);
            CREATE INDEX ix_cards_usn on cards (usn);
            CREATE INDEX ix_revlog_usn on revlog (usn);
            CREATE INDEX ix_cards_nid on cards (nid);
            CREATE INDEX ix_cards_sched on cards (did, queue, due);
            CREATE INDEX ix_revlog_cid on revlog (cid);
            CREATE INDEX ix_notes_csum on notes (csum);";

        private static readonly Dictionary<AnkiCardType, long> ModelIdOffset = new()
        {
            [AnkiCardType.Basic] = 1,
            [AnkiCardType.BasicReversed] = 2,
            [AnkiCardType.BasicOptionalReversed] = 3,
            [AnkiCardType.BasicTypeAnswer] = 4,
            [AnkiCardType.Cloze] = 5,
        };

        public static byte[] Build(List<AnkiCardModel> cards)
        {
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"anki_{Guid.NewGuid():N}.anki2");
            try
            {
                BuildSqliteFile(tempDbPath, cards);
                return ZipAsApkg(tempDbPath);
            }
            finally
            {
                // Cleanup only — a failed delete must never fail the export response.
                // The OS temp folder is swept periodically regardless of whether this succeeds.
                try
                {
                    if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                }
                catch (IOException)
                {
                    // Leftover temp file (e.g. still locked by another process); ignored intentionally.
                }
                catch (UnauthorizedAccessException)
                {
                    // Leftover temp file (e.g. permission/lock issue); ignored intentionally.
                }
            }
        }

        private static void BuildSqliteFile(string dbPath, List<AnkiCardModel> cards)
        {
            var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var todayMidnightSeconds = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long baseModelId = nowMs;               // 5 model id = baseModelId + offset (1..5)
            long deckId = nowMs + 100;

            long ModelId(AnkiCardType t) => baseModelId + ModelIdOffset[t];

            // Pooling=False: Microsoft.Data.Sqlite pools native sqlite3 connection handles by default,
            // so disposing SqliteConnection normally returns the handle to the pool instead of closing
            // it — the OS file lock on dbPath survives past the `using` block. Disabling pooling forces
            // sqlite3_close on Dispose, releasing the file handle immediately so the caller's
            // File.Delete(tempDbPath) in Build()'s finally block doesn't race a still-open handle.
            using var connection = new SqliteConnection($"Data Source={dbPath};Pooling=False");
            connection.Open();
            using (var cmd = connection.CreateCommand()) { cmd.CommandText = CreateSchemaSql; cmd.ExecuteNonQuery(); }

            string commonCss = ".card {font-family: arial; font-size: 20px; text-align: center; color: black; background-color: white;}\n.cloze {font-weight: bold; color: blue;}";
            string latexPre = "\\documentclass[12pt]{article}\n\\special{papersize=3in,5in}\n\\usepackage[utf8]{inputenc}\n\\usepackage{amssymb,amsmath}\n\\pagestyle{empty}\n\\setlength{\\parindent}{0in}\n\\begin{document}\n";
            string latexPost = "\\end{document}";

            object BasicFlds() => new[] {
                new { name = "Front", ord = 0, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() },
                new { name = "Back", ord = 1, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() }
            };

            var models = new Dictionary<string, object>
            {
                [ModelId(AnkiCardType.Basic).ToString()] = new
                {
                    id = ModelId(AnkiCardType.Basic), name = "ManageLife Basic", type = 0, mod = nowSeconds, usn = -1, sortf = 0, did = deckId,
                    tmpls = new[] { new { name = "Card 1", ord = 0, qfmt = "{{Front}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 } },
                    flds = BasicFlds(), css = commonCss, latexPre, latexPost,
                    req = new object[] { new object[] { 0, "all", new[] { 0 } } }, tags = Array.Empty<string>(), vers = Array.Empty<object>()
                },
                [ModelId(AnkiCardType.BasicReversed).ToString()] = new
                {
                    id = ModelId(AnkiCardType.BasicReversed), name = "ManageLife Basic (and reversed card)", type = 0, mod = nowSeconds, usn = -1, sortf = 0, did = deckId,
                    tmpls = new[] {
                        new { name = "Card 1", ord = 0, qfmt = "{{Front}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 },
                        new { name = "Card 2", ord = 1, qfmt = "{{Back}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Front}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 }
                    },
                    flds = BasicFlds(), css = commonCss, latexPre, latexPost,
                    req = new object[] { new object[] { 0, "all", new[] { 0 } }, new object[] { 1, "all", new[] { 1 } } }, tags = Array.Empty<string>(), vers = Array.Empty<object>()
                },
                [ModelId(AnkiCardType.BasicOptionalReversed).ToString()] = new
                {
                    id = ModelId(AnkiCardType.BasicOptionalReversed), name = "ManageLife Basic (optional reversed card)", type = 0, mod = nowSeconds, usn = -1, sortf = 0, did = deckId,
                    tmpls = new[] {
                        new { name = "Card 1", ord = 0, qfmt = "{{Front}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 },
                        new { name = "Card 2", ord = 1, qfmt = "{{#Add Reverse}}{{Back}}{{/Add Reverse}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Front}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 }
                    },
                    flds = new[] {
                        new { name = "Front", ord = 0, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() },
                        new { name = "Back", ord = 1, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() },
                        new { name = "Add Reverse", ord = 2, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() }
                    },
                    css = commonCss, latexPre, latexPost,
                    req = new object[] { new object[] { 0, "all", new[] { 0 } }, new object[] { 1, "all", new[] { 2 } } }, tags = Array.Empty<string>(), vers = Array.Empty<object>()
                },
                [ModelId(AnkiCardType.BasicTypeAnswer).ToString()] = new
                {
                    id = ModelId(AnkiCardType.BasicTypeAnswer), name = "ManageLife Basic (type in the answer)", type = 0, mod = nowSeconds, usn = -1, sortf = 0, did = deckId,
                    tmpls = new[] { new { name = "Card 1", ord = 0, qfmt = "{{Front}}\n\n{{type:Back}}", afmt = "{{FrontSide}}\n\n<hr id=answer>\n\n{{type:Back}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 } },
                    flds = BasicFlds(), css = commonCss, latexPre, latexPost,
                    req = new object[] { new object[] { 0, "all", new[] { 0 } } }, tags = Array.Empty<string>(), vers = Array.Empty<object>()
                },
                [ModelId(AnkiCardType.Cloze).ToString()] = new
                {
                    id = ModelId(AnkiCardType.Cloze), name = "ManageLife Cloze", type = 1, mod = nowSeconds, usn = -1, sortf = 0, did = deckId,
                    tmpls = new[] { new { name = "Cloze", ord = 0, qfmt = "{{cloze:Text}}", afmt = "{{cloze:Text}}<br>{{Back Extra}}", bqfmt = "", bafmt = "", did = (long?)null, bfont = "Arial", bsize = 12 } },
                    flds = new[] {
                        new { name = "Text", ord = 0, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() },
                        new { name = "Back Extra", ord = 1, sticky = false, rtl = false, font = "Arial", size = 20, media = Array.Empty<string>() }
                    },
                    css = commonCss, latexPre, latexPost,
                    req = new object[] { new object[] { 0, "all", new[] { 0 } } }, tags = Array.Empty<string>(), vers = Array.Empty<object>()
                }
            };

            var conf = JsonSerializer.Serialize(new {
                nextPos = cards.Count + 1, curDeck = deckId, activeDecks = new[] { deckId },
                curModel = ModelId(AnkiCardType.Basic).ToString(), sortType = "noteFld", sortBackwards = false,
                collapseTime = 1200, timeLim = 0, estTimes = true, dueCounts = true, newSpread = 0, dayLearnFirst = false, schedVer = 2
            });
            var modelsJson = JsonSerializer.Serialize(models);
            var decks = JsonSerializer.Serialize(new Dictionary<string, object> {
                [deckId.ToString()] = new { id = deckId, name = "ManageLife Cards", mod = nowSeconds, usn = -1,
                    lrnToday = new[] { 0, 0 }, revToday = new[] { 0, 0 }, newToday = new[] { 0, 0 }, timeToday = new[] { 0, 0 },
                    collapsed = false, browserCollapsed = false, desc = "", dyn = 0, conf = 1, extendNew = 10, extendRev = 50 }
            });
            var dconf = JsonSerializer.Serialize(new Dictionary<string, object> {
                ["1"] = new { id = 1, name = "Default", replayq = true,
                    lapse = new { delays = new[] { 10 }, mult = 0, minInt = 1, leechFails = 8, leechAction = 0 },
                    rev = new { perDay = 200, ease4 = 1.3, fuzz = 0.05, minSpace = 1, ivlFct = 1, maxIvl = 36500, bury = false },
                    timer = 0, maxTaken = 60, usn = -1,
                    @new = new { perDay = 20, delays = new[] { 1, 10 }, separate = true, ints = new[] { 1, 4, 7 }, initialFactor = 2500, bury = false, order = 1 },
                    mod = 0, autoplay = true }
            });

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO col (id,crt,mod,scm,ver,dty,usn,ls,conf,models,decks,dconf,tags) VALUES (1,$crt,$mod,$scm,11,0,0,0,$conf,$models,$decks,$dconf,'{}')";
                cmd.Parameters.AddWithValue("$crt", todayMidnightSeconds);
                cmd.Parameters.AddWithValue("$mod", nowMs);
                cmd.Parameters.AddWithValue("$scm", nowMs);
                cmd.Parameters.AddWithValue("$conf", conf);
                cmd.Parameters.AddWithValue("$models", modelsJson);
                cmd.Parameters.AddWithValue("$decks", decks);
                cmd.Parameters.AddWithValue("$dconf", dconf);
                cmd.ExecuteNonQuery();
            }

            InsertNotesAndCards(connection, cards, ModelId, deckId, nowSeconds);
        }

        private static void InsertNotesAndCards(SqliteConnection connection, List<AnkiCardModel> cards, Func<AnkiCardType, long> modelId, long deckId, long nowSeconds)
        {
            long counter = 1;
            foreach (var card in cards)
            {
                string flds;
                string sfld;
                if (card.CardType == AnkiCardType.Cloze)
                {
                    var clozeMarkup = card.FieldFront.Replace("___", "{{c1::" + card.FieldBack + "}}");
                    sfld = clozeMarkup;
                    flds = clozeMarkup + "\x1f" + (card.FieldExtra ?? "");
                }
                else if (card.CardType == AnkiCardType.BasicOptionalReversed)
                {
                    sfld = card.FieldFront;
                    flds = card.FieldFront + "\x1f" + card.FieldBack + "\x1f" + (card.FieldExtra ?? "");
                }
                else
                {
                    sfld = card.FieldFront;
                    flds = card.FieldFront + "\x1f" + card.FieldBack;
                }

                var csum = ComputeCsum(sfld);
                var noteId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + counter;
                // GUID ổn định = card.Id (không phải Guid.NewGuid() ngẫu nhiên) — để xuất lại nhiều lần cùng 1 thẻ
                // sẽ được Anki nhận diện là "cùng note" và CẬP NHẬT thay vì tạo trùng khi import lại.
                var guid = card.Id;

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO notes (id,guid,mid,mod,usn,tags,flds,sfld,csum,flags,data) VALUES ($id,$guid,$mid,$mod,-1,'',$flds,$sfld,$csum,0,'')";
                    cmd.Parameters.AddWithValue("$id", noteId);
                    cmd.Parameters.AddWithValue("$guid", guid);
                    cmd.Parameters.AddWithValue("$mid", modelId(card.CardType));
                    cmd.Parameters.AddWithValue("$mod", nowSeconds);
                    cmd.Parameters.AddWithValue("$flds", flds);
                    cmd.Parameters.AddWithValue("$sfld", sfld);
                    cmd.Parameters.AddWithValue("$csum", csum);
                    cmd.ExecuteNonQuery();
                }

                // Số card + ord sinh ra theo CardType — đây là điểm khác biệt chính giữa 5 loại
                var cardOrds = new List<int> { 0 };
                if (card.CardType == AnkiCardType.BasicReversed)
                {
                    cardOrds.Add(1); // luôn sinh cả 2 chiều
                }
                else if (card.CardType == AnkiCardType.BasicOptionalReversed && !string.IsNullOrWhiteSpace(card.FieldExtra))
                {
                    cardOrds.Add(1); // chỉ sinh chiều ngược nếu người dùng có tick "tạo thẻ đảo chiều"
                }

                foreach (var ord in cardOrds)
                {
                    var cardId = noteId + 1000 + ord; // tách khỏi vùng id note để không trùng
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "INSERT INTO cards (id,nid,did,ord,mod,usn,type,queue,due,ivl,factor,reps,lapses,left,odue,odid,flags,data) VALUES ($id,$nid,$did,$ord,$mod,-1,0,0,$due,0,0,0,0,0,0,0,0,'')";
                    cmd.Parameters.AddWithValue("$id", cardId);
                    cmd.Parameters.AddWithValue("$nid", noteId);
                    cmd.Parameters.AddWithValue("$did", deckId);
                    cmd.Parameters.AddWithValue("$ord", ord);
                    cmd.Parameters.AddWithValue("$mod", nowSeconds);
                    cmd.Parameters.AddWithValue("$due", counter);
                    cmd.ExecuteNonQuery();
                }
                counter++;
            }
        }

        private static long ComputeCsum(string sortField)
        {
            var hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(sortField));
            var hex8 = Convert.ToHexString(hashBytes).ToLowerInvariant().Substring(0, 8);
            return Convert.ToInt64(hex8, 16);
        }

        private static byte[] ZipAsApkg(string dbPath)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var dbEntry = archive.CreateEntry("collection.anki2", CompressionLevel.Fastest);
                using (var entryStream = dbEntry.Open())
                using (var fileStream = File.OpenRead(dbPath))
                    fileStream.CopyTo(entryStream);

                var mediaEntry = archive.CreateEntry("media", CompressionLevel.Fastest);
                using (var entryStream = mediaEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                    writer.Write("{}");
            }
            return memoryStream.ToArray();
        }
    }
}
