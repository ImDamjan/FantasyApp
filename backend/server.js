const express = require('express');
const connectDB = require('./config/config');

const app = express();

// Connect to Database
connectDB();

// Middleware
app.use(express.json()); // For parsing application/json

// Basic route
app.get('/', (req, res) => {
  res.send('Welcome to the Fantasy App API');
});

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
