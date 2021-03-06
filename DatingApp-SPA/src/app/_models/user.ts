import { Photo } from './photo';

export interface User {
    id: number;
    userName: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: any;
    city: string;
    country: string;
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photoUrl?: string;
    photos?: Photo[];
    roles?: string[];
}
