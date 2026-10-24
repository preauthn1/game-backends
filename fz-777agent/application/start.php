<?php

require ANF_PATH . '/vendor/autoload.php';

define("APP_ROOT",dirname(dirname(__FILE__)));
define("ASSETS_ROOT",APP_ROOT.'/public/assets');
// Instantiate the app
$settings = require APP_PATH . '/configs/base.php';
$app      = new \Slim\App($settings);

$container = $app->getContainer();

/*
$app->add(new \Slim\Middleware\Session([
'name'     => 'tencent_session',
'lifetime' => '20 minutes',
]));

$container['session'] = function ($c) {
return new \SlimSession\Helper;
};

//$container['session']->startSession();
//session_start();
//session_unset();
//session_destroy();
//session_write_close();

$container['session']->set('aiensis', 'god');
//echo $container['session']->get('aiensis');
 */

/*$session = new \Slim\Middleware\Session([
'name' => 'tencent_session',
]);*/
session_start();

$container['session'] = function ($c) {
    return new \SlimSession\Helper;
};
//$container['session']->set('aiensis', 'god');

$container['view'] = function ($container) {
    $settings = $container->get('settings')['renderer'];
    $view     = new \Slim\Views\Twig($settings['templatePath'], [
        'cache' => $settings['templateCachePath'],
        'debug' => 'auto_reload',
    ]);
    $view->addExtension(new \Slim\Views\TwigExtension(
        $container['router'],
        $container['request']->getUri()
    ));

    return $view;
};

//注册monolog日志
$container['logger'] = function($c) {
    $setting = $c->get('settings')['logger'];
    $logger = new \Monolog\Logger($setting['name']);
    $path = '../logs/' . date('Y-m-d') . 'app.log';
    $fileHandler = new \Monolog\Handler\StreamHandler($path, $setting['level']);
    $logger->pushHandler($fileHandler);
    return $logger;
};

$capsule = new \Illuminate\Database\Capsule\Manager;
$capsule->addConnection($container['settings']['log'],'log');
$capsule->addConnection($container['settings']['center'],'center');
$capsule->addConnection($container['settings']['perGameLog'],'perGameLog');
$capsule->addConnection($container['settings']['db']);
$capsule->addConnection($container['settings']['notify'],'notify');
$capsule->setAsGlobal();
$capsule->bootEloquent();
$container['db'] = $capsule;

require APP_PATH . '/configs/app.php';
require APP_PATH . '/configs/routes.php';
require APP_PATH . '/constants/const.php';

//$wechat = require APP_PATH . '/configs/wechat.php';
//$container['wechat'] = new \EasyWeChat\Foundation\Application($wechat);

/*$container[App\Controllers\BaseController::class] = function ($c) {
    return new App\Controllers\BaseController($c->get('db'));
};*/
// Run app
$app->run();
