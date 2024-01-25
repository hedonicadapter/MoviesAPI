class HttpError extends Error {
  constructor(message, statusCode) {
    super(message); // Pass the message to the Error constructor
    this.statusCode = statusCode;
  }
}

module.exports = HttpError;
