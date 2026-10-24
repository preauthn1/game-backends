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
 * Environment Interface
 *
 * @package Slim
 * @since   3.0.0
 */
interface EnvironmentInterface
{
    public static function mock(array $settings = []);
}
