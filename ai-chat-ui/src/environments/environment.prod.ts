export const environment = {
  production: false,
  apiUrl: 'https://localhost:7045/api/',
  msalConfig: {
    auth: {
      clientId: 'b40defa0-5309-45c4-82fc-cb284010cc10',
      authority:
        'https://login.microsoftonline.com/bdeeb99b-f1e9-443f-b7ff-1426199689dc',
    },
  },
  apiConfig: {
    scopes: ['b40defa0-5309-45c4-82fc-cb284010cc10/.default'],
    uri: 'https://localhost:7045/api/',
  },
};
