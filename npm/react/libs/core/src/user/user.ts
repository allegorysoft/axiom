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
  user: User;
}

export interface UserStore extends AxiomStore<UserState> {
  setUser(user: User | undefined): void;
  patchUser(user: Partial<User>): void;
}
