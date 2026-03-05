import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';

type UserState = {
  isInitialized: boolean;
};

const initialState: UserState = {
  isInitialized: false,
};

export const UserStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    /**
     * Marks the user as successfully initialized after createUser() resolves.
     */
    setInitialized(): void {
      patchState(store, { isInitialized: true });
    },
  })),
);
