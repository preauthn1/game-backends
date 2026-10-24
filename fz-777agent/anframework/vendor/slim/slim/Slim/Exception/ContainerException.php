<?php
/**
 * Slim Framework (http://example.invalid)
 *
 * @link      https://example.invalid/codeguy/Slim
 * @copyright Copyright (c) 2011-2015 Josh Lockhart
 * @license   https://example.invalid/codeguy/Slim/blob/master/LICENSE (MIT License)
 */
namespace Slim\Exception;

use InvalidArgumentException;
use Interop\Container\Exception\ContainerException as InteropContainerException;

/**
 * Container Exception
 */
class ContainerException extends InvalidArgumentException implements InteropContainerException
{

}
