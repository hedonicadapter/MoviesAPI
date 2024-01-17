const express = require('express');
const path = require('path');

const app = express();
const port = 3000;

app.use('/static', express.static(path.join(__dirname, 'public')));

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

app.get('/edit', (req, res) => {
  res.sendFile(__dirname + '/pages/edit.html');
});

app.listen(port, () => console.log(`Example app listening on port ${port}!`));
