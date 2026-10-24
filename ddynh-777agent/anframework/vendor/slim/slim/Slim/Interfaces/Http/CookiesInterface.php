<?php
/**
 * Slim Framework (http://example.invalid)
 *
 * @link      https://example.invalid/slimphp/Slim
 * @copyright Copyright (c) 2011-2016 Josh Lockhart
 * @license   https://example.invalid/slimphp/Slim/blob/3.x/LICENSE.md (MIT License)
 */
namespace Slim\Interfaces\Http;

/**
 * Cookies Interface
 *
 * @package Slim
 * @since   3.0.0
 */
interface CookiesInterface
{
    public function get($name, $default = null);
    public function set($name, $value);
    public function toHeaders();
    public static function parseHeader($header);
}
