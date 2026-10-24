<?php
/**
 * Slim Framework (http://example.invalid)
 *
 * @link      https://example.invalid/slimphp/Slim
 * @copyright Copyright (c) 2011-2016 Josh Lockhart
 * @license   https://example.invalid/slimphp/Slim/blob/3.x/LICENSE.md (MIT License)
 */
namespace Slim\Interfaces\Http;

use Slim\Interfaces\CollectionInterface;

/**
 * Headers Interface
 *
 * @package Slim
 * @since   3.0.0
 */
interface HeadersInterface extends CollectionInterface
{
    public function add($key, $value);

    public function normalizeKey($key);
}
