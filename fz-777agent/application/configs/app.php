<?php
ini_set('date.timezone', 'Asia/Shanghai');
$serverParams = $container['request']->getServerParams();

define('SYS_TIME', $serverParams['REQUEST_TIME']);

define('SITE_PROTOCOL', isset($serverParams['SERVER_PORT']) && $serverParams['SERVER_PORT'] == '443' ? 'https://' : 'http://');

//$serverport=$serverParams['SERVER_PORT']=='80'?'':':'.$serverParams['SERVER_PORT'];
define('SYS_PATH', SITE_PROTOCOL . $serverParams['HTTP_HOST']);
define('APP_URL', SYS_PATH);
define('SELF_URL', $container['request']->getUri());
define('ASSETS_PATH', SYS_PATH.'/assets');
define('DATA_PATH', SYS_PATH.'/data');
define('UPLOAD_PATH', SYS_PATH.'/data/uploads');
define('BJUI_PATH', SYS_PATH.'/bjui');

/* $container['csrf'] = function ($c) {
    $csrfExcept = ['/ajax/upload?type=image'];
    return new \Slim\Csrf\Guard($csrfExcept);
};
$app->add($container->get('csrf')); */

$queryParams              = $container->request->getQueryParams();
$container['queryParams'] = $queryParams;
$viewArgs                 = [
    'APP_URL'     => APP_URL,
    'SYS_PATH'    => SYS_PATH,
    'SELF_URL'    => SELF_URL,
    'ASSETS_PATH' => ASSETS_PATH,
    'BJUI_PATH'   => BJUI_PATH,
    'DATA_PATH'   => DATA_PATH,
    'args'        => $queryParams,
];

$container['view']['viewArgs'] = $viewArgs;
$container['view']['twig']     = $container['view']->getEnvironment();

foreach ($container['view']['viewArgs'] as $key => $value) {
    $container['view']['twig']->addGlobal($key, $value);
}
