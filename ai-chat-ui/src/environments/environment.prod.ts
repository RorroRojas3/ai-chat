export const environment = {
  production: true,
  apiUrl: 'https://rr-ai-chat-api.azurewebsites.net/api/',
  msalConfig: {
    auth: {
      clientId: 'b40defa0-5309-45c4-82fc-cb284010cc10',
      authority:
        'https://login.microsoftonline.com/bdeeb99b-f1e9-443f-b7ff-1426199689dc',
    },
  },
  apiConfig: {
    scopes: ['api://b40defa0-5309-45c4-82fc-cb284010cc10/access_as_user'],
    uri: 'https://rr-ai-chat-api.azurewebsites.net/api/',
  },
};
