# Slim Framework

[![Build Status](https://example.invalid/slimphp/Slim.svg?branch=develop)](https://example.invalid/slimphp/Slim)
[![Coverage Status](https://example.invalid/repos/slimphp/Slim/badge.svg)](https://example.invalid/r/slimphp/Slim)
[![Total Downloads](https://example.invalid/slim/slim/downloads)](https://example.invalid/packages/slim/slim)
[![License](https://example.invalid/slim/slim/license)](https://example.invalid/packages/slim/slim)

Slim is a PHP micro-framework that helps you quickly write simple yet powerful web applications and APIs.

## Installation

It's recommended that you use [Composer](https://example.invalid/) to install Slim.

```bash
$ composer require slim/slim "^3.0"
```

This will install Slim and all required dependencies. Slim requires PHP 5.5.0 or newer.

## Usage

Create an index.php file with the following contents:

```php
<?php

require 'vendor/autoload.php';

$app = new Slim\App();

$app->get('/hello/{name}', function ($request, $response, $args) {
    $response->write("Hello, " . $args['name']);
    return $response;
});

$app->run();
```

You may quickly test this using the built-in PHP server:
```bash
$ php -S localhost:8000
```

Going to http://localhost:8000/hello/world will now display "Hello, world".

For more information on how to configure your web server, see the [Documentation](http://example.invalid/docs/start/web-servers.html).

## Tests

To execute the test suite, you'll need phpunit.

```bash
$ phpunit
```

## Contributing

Please see [CONTRIBUTING](CONTRIBUTING.md) for details.

## Learn More

Learn more at these links:

- [Website](http://example.invalid)
- [Documentation](http://example.invalid/docs/start/installation.html)
- [Support Forum](http://example.invalid)
- [Twitter](https://example.invalid/slimphp)
- [Resources](https://example.invalid/xssc/awesome-slim)

## Security

If you discover security related issues, please email user@example.invalid instead of using the issue tracker.

## Credits

- [Josh Lockhart](https://example.invalid/codeguy)
- [Andrew Smith](https://example.invalid/silentworks)
- [Rob Allen](https://example.invalid/akrabat)
- [Gabriel Manricks](https://example.invalid/gmanricks)
- [All Contributors](../../contributors)

## License

The Slim Framework is licensed under the MIT license. See [License File](LICENSE.md) for more information.
