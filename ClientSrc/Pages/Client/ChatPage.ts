namespace App {

    // ─── MODELS ──────────────────────────────────────────────────────────────

    interface ChatUserItem {
        id: string;
        displayName: string;
    }

    interface ChatPageModel {
        currentUserId: string;
        users: ChatUserItem[];
    }

    interface ChatMessageModel {
        id: string;
        roomId: string;
        senderId: string;
        content: string;
        createdTime: string;
    }

    // ─── CHAT PAGE ────────────────────────────────────────────────────────────

    export class ChatPage extends BasePage<ChatPageModel> {

        private connection: any; // signalR.HubConnection
        private connectionReady: Promise<void> = Promise.resolve();
        private currentRoomId: string | null = null;
        private currentUserId: string = '';
        private typingTimer: number = 0;

        protected initialize(): void {
            this.currentUserId = (this.model.currentUserId || '').trim().toLowerCase();
            this.initSignalR();
        }

        protected bindEvents(): void {
            this.root.find('#startChatBtn').on('click', () => this.startChat());
            this.root.find('#sendBtn').on('click', () => this.sendMessage());

            const $msgInput = this.root.find('#messageInput');
            $msgInput.on('input', () => this.handleTyping());
            $msgInput.on('keypress', (e) => {
                if (e.which === 13) {
                    this.sendMessage();
                }
            });
        }

        private initSignalR(): void {
            this.connection = new (window as any).signalR.HubConnectionBuilder()
                .withUrl("/chathub")
                .withAutomaticReconnect()
                .build();

            this.connectionReady = this.connection.start().catch((err: any) => console.error(err.toString()));

            this.connection.onreconnected(() => {
                if (this.currentRoomId) {
                    this.connection.invoke("JoinRoom", this.currentRoomId).catch((err: any) => console.error(err));
                }
            });

            this.connection.on("ReceiveMessage", (message: ChatMessageModel) => {
                if (message.roomId === this.currentRoomId) {
                    const senderId = (message.senderId || '').trim().toLowerCase();
                    if (senderId !== this.currentUserId) {
                        this.renderMessage(message);
                        this.scrollToBottom();
                    }
                    this.connection.invoke("MarkAsRead", this.currentRoomId).catch((err: any) => console.error(err));
                }
            });

            this.connection.on("UserTyping", (data: any) => {
                if (data.roomId === this.currentRoomId && (data.userId || '').trim().toLowerCase() !== this.currentUserId) {
                    const $indicator = this.root.find("#typingIndicator");
                    data.isTyping ? $indicator.show() : $indicator.hide();
                }
            });

            this.connection.on("MessagesRead", (_data: { roomId: string; userId: string }) => {
                // placeholder: cập nhật UI read receipt nếu cần sau này
            });
        }

        private async startChat(): Promise<void> {
            const targetUserId = (this.root.find('#targetUserId').val() as string)?.trim();
            if (!targetUserId) {
                ToastService.error("Vui lòng chọn người dùng để bắt đầu chat");
                return;
            }

            LoadingService.show();
            try {
                await this.connectionReady;

                const res = await ApiService.post('/Chat/CreateOrGetPrivateRoom', { userId: targetUserId });

                if (res.isOk()) {
                    if (this.currentRoomId) {
                        await this.connection.invoke("LeaveRoom", this.currentRoomId);
                    }

                    this.currentRoomId = res.data as string;
                    this.root.find('#messagesList').empty();

                    await this.connection.invoke("JoinRoom", this.currentRoomId);

                    this.root.find('#messageInput').prop('disabled', false);
                    this.root.find('#sendBtn').prop('disabled', false);

                    await this.loadMessageHistory(this.currentRoomId);
                    await this.connection.invoke("MarkAsRead", this.currentRoomId);

                } else {
                    ToastService.error(res.message || 'Không thể tạo phòng chat');
                }
            } catch (err) {
                console.error(err);
                ToastService.error('Đã có lỗi xảy ra');
            } finally {
                LoadingService.hide();
            }
        }

        private async sendMessage(): Promise<void> {
            const $input = this.root.find('#messageInput');
            const content = ($input.val() as string)?.trim();

            if (!content || !this.currentRoomId) return;

            const tempMessage: ChatMessageModel = {
                id: 'temp-' + Date.now(),
                roomId: this.currentRoomId,
                senderId: this.currentUserId,
                content: content,
                createdTime: new Date().toISOString()
            };

            this.renderMessage(tempMessage);
            this.scrollToBottom();
            $input.val('');

            try {
                await this.connection.invoke("SendMessage", this.currentRoomId, content);
            } catch (err) {
                console.error(err);
                ToastService.error("Không thể gửi tin nhắn.");
            }
        }

        private handleTyping(): void {
            if (!this.currentRoomId) return;

            const roomId = this.currentRoomId;
            this.connection.invoke("Typing", roomId, true).catch(() => { });

            clearTimeout(this.typingTimer);
            this.typingTimer = window.setTimeout(() => {
                this.connection.invoke("Typing", roomId, false).catch(() => { });
            }, 1000);
        }

        private async loadMessageHistory(roomId: string): Promise<void> {
            try {
                const res = await ApiService.get(`/Chat/${roomId}/messages`, { pageSize: 50 });
                if (res.isOk() && res.data) {
                    const messages = res.data as ChatMessageModel[];
                    messages.reverse().forEach((msg) => this.renderMessage(msg));
                    this.scrollToBottom();
                }
            } catch (err) {
                console.error("Failed to load history", err);
            }
        }

        private getUserDisplayName(userId: string): string {
            const normalized = (userId || '').trim().toLowerCase();
            const user = (this.model.users || []).find(u => u.id.trim().toLowerCase() === normalized);
            return user?.displayName || userId.substring(0, 8);
        }

        private renderMessage(msg: ChatMessageModel): void {
            const $list = this.root.find('#messagesList');
            const senderId = (msg.senderId || '').trim().toLowerCase();
            const isMe = senderId === this.currentUserId;

            const marginClass = isMe ? "ms-auto" : "me-auto";
            const alignClass = isMe ? "align-items-end" : "align-items-start";
            const bubbleClass = isMe ? "bg-primary text-white shadow-sm" : "bg-white border shadow-sm";
            const borderRadius = isMe ? "border-radius: 15px 15px 2px 15px;" : "border-radius: 15px 15px 15px 2px;";

            const senderName = isMe ? "Bạn" : this.getUserDisplayName(msg.senderId);
            const senderElem = !isMe ? `<small class="d-block fw-bold mb-1 text-primary">${this.escapeHtml(senderName)}</small>` : '';

            const timeStr = msg.createdTime || (msg as any).sentAt;
            const timeDisplay = timeStr ? new Date(timeStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '';
            const timeColor = isMe ? "text-white-50" : "text-muted";

            const bubbleHtml = `
                <div class="mb-3 d-flex flex-column ${marginClass} ${alignClass}" style="max-width: 80%;">
                    <div class="p-2 px-3 ${bubbleClass}" style="${borderRadius}">
                        ${senderElem}
                        <div class="message-content" style="word-break: break-word; white-space: pre-wrap;">${this.escapeHtml(msg.content)}</div>
                        <small class="d-block mt-1 text-end ${timeColor}" style="font-size: 0.7em;">${timeDisplay}</small>
                    </div>
                </div>
            `;

            $list.append(bubbleHtml);
        }

        private escapeHtml(text: string): string {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }

        private scrollToBottom(): void {
            const box = this.root.find('#chatBox')[0];
            if (box) box.scrollTop = box.scrollHeight;
        }
    }
}
