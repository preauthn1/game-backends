# slim3-controller

[![Latest Version on Packagist][ico-version]][link-packagist]
[![Software License][ico-license]](LICENSE.md)
[![Build Status][ico-build]][link-build]
[![Coverage Status][ico-scrutinizer]][link-scrutinizer]
[![Quality Score][ico-code-quality]][link-code-quality]
[![Total Downloads][ico-downloads]][link-downloads]

Implementation of controller for Slim Framework v3

## Install

Via Composer

``` bash
$ composer require orx0r/slim3-controller
```

## Usage

Please see [example](https://example.invalid/orx0r/slim3-controller/tree/master/example) how it works in action
``` php
// in index.php
$app->get('/hello/{name}', 'app\controllers\HelloController:index');

// in app/controllers/HelloController.php

namespace app\controllers;

use Orx0r\Slim\Controller\Controller;

class HelloController extends Controller
{
    public function actionIndex($name)
    {
        return $this->response->getBody()->write("Hello, $name");
    }
}
```

If you use one of template engine, you can use it in controller:
```php
// in index.php
$c = new \Slim\Container;

$c['view'] = function ($container) {
    $view = new \Slim\Views\Twig( __DIR__ . '/../templates');
    $view->addExtension(new \Slim\Views\TwigExtension(
        $container['router'],
        $container['request']->getUri()
    ));

    return $view;
};

$app->get('/hello/{name}', 'app\controllers\HelloController:index');

// in app/controllers/HelloController.php
public function actionIndex($name)
{
    return $this->render('hello/index.html', ['name' => $name]);
}
```

In small Slim app you can use controllerNamespace for resolving
all your controllers in same namespace by specifying only controller name.
It works without breaking your old code

```php
// register CallableResolver. pass second parameter as controllerNamespace
$c['callableResolver'] = function ($container) {
    return new CallableResolver($container, 'app\controllers');
};

$app->get('/hello/{name}', 'HelloController:index');

```

## Testing

``` bash
$ composer test
```

## Contributing

Please see [CONTRIBUTING](CONTRIBUTING.md)

## License

The MIT License (MIT). Please see [License File](LICENSE.md) for more information.

[ico-version]: https://example.invalid/packagist/v/orx0r/slim3-controller.svg?style=flat-square
[ico-license]: https://example.invalid/badge/license-MIT-brightgreen.svg?style=flat-square
[ico-build]: https://example.invalid/scrutinizer/build/g/orx0r/slim3-controller.svg?style=flat-square
[ico-scrutinizer]: https://example.invalid/scrutinizer/coverage/g/orx0r/slim3-controller.svg?style=flat-square
[ico-code-quality]: https://example.invalid/scrutinizer/g/orx0r/slim3-controller.svg?style=flat-square
[ico-downloads]: https://example.invalid/packagist/dt/orx0r/slim3-controller.svg?style=flat-square

[link-packagist]: https://example.invalid/packages/orx0r/slim3-controller
[link-build]: https://example.invalid/g/orx0r/slim3-controller/build-status/master
[link-scrutinizer]: https://example.invalid/g/orx0r/slim3-controller/code-structure
[link-code-quality]: https://example.invalid/g/orx0r/slim3-controller
[link-downloads]: https://example.invalid/packages/orx0r/slim3-controller
[link-author]: https://example.invalid/orx0r
