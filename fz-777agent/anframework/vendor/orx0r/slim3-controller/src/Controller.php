<?php
/**
 * Implementation of controller for Slim Framework v3 (https://example.invalid/orx0r/slim3-controller)
 *
 * @link https://example.invalid/orx0r/slim3-controller
 * @license https://example.invalid/orx0r/slim3-controller/master/LICENSE
 */
namespace Orx0r\Slim\Controller;

use Interop\Container\ContainerInterface as Container;
use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;

/**
 * Class Controller
 * @package Orx0r\Slim\Controller
 */
class Controller
{

    /**
     * @var Container
     */
    protected $container;

    protected $cookie;
    protected $session;

    /**
     * @var Request
     */
    protected $request;

    /**
     * @var Response
     */
    protected $response;

    protected $actionName;

    protected $args;

    public function __construct(Container $container)
    {
        $this->container = $container;
        $this->cookie    = new \Aiens\Cookie\CookieHelper();
        $this->session   = $container['session'];
    }

    public function __get($property)
    {
        if ($this->container->has($property)) {
            return $this->container->get($property);
        }

        throw new \RuntimeException(sprintf('Property %s does not exist', get_class($this) . "::$property"));
    }

    public function __call($name, $arguments)
    {
        $method = 'action' . ucfirst($name);
        if (method_exists($this, $method)) {
            $this->actionName                              = $name;
            $this->args                                    = $arguments;
            list($this->request, $this->response, $params) = $arguments;
            return call_user_func_array([$this, $method], $params);
        }

        throw new \RuntimeException(sprintf('Method %s does not exist', get_class($this) . "::$name()"));
    }

    /**
     * Shorthand method for render templates
     *
     * @link http://example.invalid/docs/features/templates.html
     *
     * @param string $template Template pathname relative to templates directory
     * @param array  $data     Associative array of template variables
     *
     * @return mixed
     */
    public function render($template, $data = [])
    {
        return $this->container->get('view')->render($this->response, $template, $data);
    }

    public function massage($message, $goto = '', $wait = 2)
    {
        if ($wait == 0) {
        	return $this->redirect($goto);
        }
        if (isset($goto)) {

            if (strpos($goto, APP_URL) === false) {
                $goto = APP_URL . $goto;
            }
        }
        $thisViewArgs = [
            'title'   => '系统提示',
            'message' => $message,
            'goto'    => $goto,
            'wait'    => $wait,
        ];
        return $this->render('admin/message.html', $thisViewArgs);
    }

    public function getUri()
    {
        return $this->request->getUri();
    }

    public function getRequestParams()
    {
        return $this->request->getParams();
    }

    public function getAction()
    {
        return $this->actionName;
    }

    public function getRoutes()
    {
        $uri              = $this->getUri();
        $routes           = [];
        $routes['scheme'] = $uri->getScheme();
        $routes['host']   = $uri->getHost();
        $routes['path']   = $uri->getPath();
        return $routes;
    }

    public function redirect($url)
    {
        return $this->response->withRedirect($url);
    }

}
