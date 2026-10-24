<?php
/**
 * Slim Framework (http://example.invalid)
 *
 * @link      https://example.invalid/codeguy/Slim
 * @copyright Copyright (c) 2011-2016 Josh Lockhart
 * @license   https://example.invalid/codeguy/Slim/blob/master/LICENSE (MIT License)
 */
namespace Slim\Exception;

use RuntimeException;
use Interop\Container\Exception\NotFoundException as InteropNotFoundException;

/**
 * Not Found Exception
 */
class ContainerValueNotFoundException extends RuntimeException implements InteropNotFoundException
{

}
