document.querySelector('form').addEventListener('submit', function (event) {
  event.preventDefault();

  document.querySelectorAll('input').forEach(function (input) {
    if (
      input.type === 'text' ||
      input.type === 'number' ||
      input.type === 'hidden' ||
      input.type === 'textarea' ||
      input.type === 'password' ||
      input.type === 'email' ||
      input.type === 'tel' ||
      input.type === 'url' ||
      input.type === 'search'
    ) {
      input.value = sanitize(input.value);
    }
  });

  event.target.submit();
});

function sanitize(input) {
  var output = input
    .replace(/<script[^>]*?>.*?<\/script>/gi, '')
    .replace(/<[\/\!]*?[^<>]*?>/gi, '')
    .replace(/<style[^>]*?>.*?<\/style>/gi, '')
    .replace(/<![\s\S]*?--[ \t\n\r]*>/gi, '');
  return output.trim();
}
