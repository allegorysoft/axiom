import { AxiomStore } from '../models';

export interface User {
  name: string;
  firstName: string;
  surname: string;
  timeZone: string;
  username: string;
  email: string;
  phone: string;
  photo: string;
}

export interface UserState {
  user: User | null;
}

export interface UserStore extends AxiomStore<UserState> {
  setUser(user: User | null): void;
  patchUser(user: Partial<User>): void;
}
