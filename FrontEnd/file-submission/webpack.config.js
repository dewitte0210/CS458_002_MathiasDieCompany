const Dotenv = require('dotenv-webpack');

module.exports = {
  plugins: [
    new Dotenv({
      path:"/.env",
      systemvars: true
    }),
  ],
  
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader', 
          options: {
          }
        }
      }
    ]
  }
};