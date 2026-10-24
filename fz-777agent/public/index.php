<?php
//echo phpinfo();exit;
/**
 * Anframework All needs framework
 *
 * @package  Anframework
 * @author   Aiens <user@example.invalid>
 */

ini_set('display_errors', '1');

define('APP_PATH', __DIR__ . '/../application');
define('ANF_PATH', __DIR__ . '/../anframework');

require APP_PATH . '/start.php';
