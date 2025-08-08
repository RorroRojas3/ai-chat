// Example Environment Configuration for AI Chat UI
// Copy this file for different environments (environment.prod.ts, environment.staging.ts, etc.)

export const environment = {
  production: false,
  
  // API Configuration
  apiUrl: 'https://localhost:7045/api/',
  
  // Alternative for production:
  // apiUrl: 'https://your-api-domain.com/api/',
  
  // Alternative for different local API port:
  // apiUrl: 'http://localhost:5045/api/',
};

// Production Environment Example:
// export const environment = {
//   production: true,
//   apiUrl: 'https://your-production-api.com/api/',
// };

// Development with HTTP (if HTTPS issues):
// export const environment = {
//   production: false,
//   apiUrl: 'http://localhost:5045/api/',
// };