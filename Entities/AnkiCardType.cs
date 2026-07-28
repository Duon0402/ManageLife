namespace ManageLife.Entities
{
    public enum AnkiCardType
    {
        Basic = 0,
        BasicReversed = 1,        // Basic (and reversed card)
        BasicOptionalReversed = 2, // Basic (optional reversed card)
        BasicTypeAnswer = 3,      // Basic (type in the answer)
        Cloze = 4
    }
}
