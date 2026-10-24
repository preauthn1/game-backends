<?php

namespace App\Loggers;

use Illuminate\Log\Logger;
use App\Loggers\JsonFormatter;

class ApplogFormatter
{
    /**
     * Customize the given logger instance.
     */
    public function __invoke(Logger $logger): void
    {
        foreach ($logger->getHandlers() as $handler) {
            $handler->setFormatter(new JsonFormatter());
        }
    }
}
