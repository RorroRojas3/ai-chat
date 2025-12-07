export const environment = {
  production: true,
  apiUrl: 'https://localhost:7045/api/',
  msalConfig: {
    auth: {
      clientId: 'cb416c88-14b7-4a39-a18e-7a134a0568b5',
      authority:
        'https://login.microsoftonline.com/e2c96319-7527-42d1-9706-49d5fcabb554',
    },
  },
  apiConfig: {
    scopes: ['api://cb416c88-14b7-4a39-a18e-7a134a0568b5/access_as_user'],
    uri: 'https://localhost:7045/api/',
  },
};
