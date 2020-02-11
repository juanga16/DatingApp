export interface Message {
    id: number;
    senderId: number;
    senderKnownAs: string;
    senderPhotoUrl: string;
    recipientId: number;
    recipientKnownAs: number;
    recipientPhotoUrl: number;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}
