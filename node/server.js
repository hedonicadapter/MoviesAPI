const express = require('express');
const path = require('path');
const HttpError = require('./util');

const port = 3000;
const app = express();

app.set('view engine', 'pug');
app.set('views', path.join(__dirname, 'views'));

app.use(express.urlencoded({ extended: true }));
app.use(express.json());

app.use('/static', express.static(path.join(__dirname, 'public')));

app.get('/api-docs', (req, res) => {
  res.sendFile(path.join(__dirname, 'openapi-spec.yaml'));
});

app.get('/redoc', (req, res) => {
  res.send(`
    <!DOCTYPE html>
    <html>
      <head>
        <title>ReDoc</title>
        <!-- needed for adaptive design -->
        <meta charset="utf-8"/>
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <link href="https://fonts.googleapis.com/css?family=Montserrat:300,400,700|Roboto:300,400,700" rel="stylesheet">

        <!--
        ReDoc doesn't change outer page styles
        -->
        <style>
          body {
            margin: 0;
            padding: 0;
          }
        </style>
      </head>
      <body>
        <redoc spec-url='/api-docs'></redoc>
        <script src="https://cdn.jsdelivr.net/npm/redoc@next/bundles/redoc.standalone.js"> </script>
      </body>
    </html>
  `);
});

app.get('/', (req, res) => {
  res.render('index', { currentRoute: req.path });
});

app.get('/edit', (req, res) => {
  const fromHtmx = req.headers['hx-request'];

  if (fromHtmx) {
    res.render('find-movie-form');
  } else {
    res.render('index', { currentRoute: req.path });
  }
});

app.get('/api/movies/random', async (req, res) => {
  try {
    const response = await fetch('http://localhost:5275/api/movies/random');
    if (!response.ok) throw new Error(response.status);

    const movieJson = await response.json();
    if (!movieJson) throw new HttpError('Movie not found', 404);

    res.render('movie', movieJson);
  } catch (ex) {
    res.status(ex.status || 500).render('error', { error: ex });
  }
});

app.get('/api/movies/findMovie', async (req, res) => {
  try {
    const { Title, Year } = req.query;

    const response = await fetch(
      `http://localhost:5275/api/movies/findMovie?title=${Title}&year=${Year}`
    );
    if (!response.ok) throw new Error(response.status);

    const movieJson = await response.json();
    if (!movieJson) throw new HttpError('Movie not found', 404);

    res.render('found-movie-form', movieJson);
  } catch (ex) {
    res.status(ex.status || 500).render('error', { error: ex });
  }
});

app.put('/api/movies', async (req, res) => {
  try {
    const formData = req.body;

    const response = await fetch('http://localhost:5275/api/movies', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: new URLSearchParams(formData),
    });
    if (!response.ok) throw new Error(response.status);

    res.render('movie', formData);
  } catch (ex) {
    res.status(ex.status || 500).render('error', { error: ex });
  }
});

app.get('/api/movies/findMovieForm', (req, res) => {
  res.render('find-movie-form');
});

app.get('/api/movies/:id', async (req, res) => {
  try {
    const id = req.params.id;

    const response = await fetch(`http://localhost:5275/api/movies/${id}`);
    if (!response.ok) throw new Error(response.status);

    const movieJson = await response.json();
    if (!movieJson) throw new HttpError('Movie not found', 404);

    res.render('found-movie-form', movieJson);
  } catch (ex) {
    res.status(ex.status || 500).render('error', { error: ex });
  }
});

app.delete('/api/movies/:id', async (req, res) => {
  const id = req.params.id;

  try {
    const response = await fetch(`http://localhost:5275/api/movies/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error(response.status);

    res.redirect('/');
  } catch (ex) {
    res.status(ex.status || 500).render('error', { error: ex });
  }
});

app.listen(port, () => console.log(`Example app listening on port ${port}!`));
