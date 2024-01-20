const express = require('express');
const path = require('path');

const port = 3000;
const app = express();
app.set('view engine', 'pug');
app.set('views', path.join(__dirname, 'views'));

app.use(express.urlencoded({ extended: true }));
app.use(express.json());

app.use('/static', express.static(path.join(__dirname, 'public')));

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

app.get('/edit', (req, res) => {
  res.sendFile(__dirname + '/pages/edit.html');
});

app.get('/api/movies/random', async (req, res) => {
  try {
    const movie = await fetch('http://localhost:5275/api/movies/random');
    const movieJson = await movie.json();

    if (!movieJson) {
      res.render('error');
    }

    res.render('movie', movieJson);
  } catch (ex) {
    res.render('error', { error: ex });
  }
});

app.get('/api/movies/findMovie', async (req, res) => {
  try {
    const { Title, Year } = req.query;

    const movie = await fetch(
      `http://localhost:5275/api/movies/findMovie?title=${Title}&year=${Year}`
    );
    const movieJson = await movie.json();

    if (!movieJson) {
      res.render('error');
    }

    res.render('found-movie-form', movieJson);
  } catch (ex) {
    res.render('error', { error: ex });
  }
});

app.put('/api/movies', async (req, res) => {
  try {
    const formData = req.body;

    const response = await fetch('http://localhost:5275/api/movies', {
      method: 'PUT',
      body: formData,
    });
    const movieJson = await response.json();

    if (!movieJson) {
      return res.render('error');
    }

    res.render('movie', formData);
  } catch (ex) {
    res.render('error', { error: ex });
  }
});

app.get('/api/movies/findMovieForm', (req, res) => {
  res.render('find-movie-form');
});

app.listen(port, () => console.log(`Example app listening on port ${port}!`));
