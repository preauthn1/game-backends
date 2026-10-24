<?php

/*
 * This file is part of Twig.
 *
 * (c) 2011 Fabien Potencier
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

/**
 * Imports blocks defined in another template into the current template.
 *
 * <pre>
 * {% extends "base.html" %}
 *
 * {% use "blocks.html" %}
 *
 * {% block title %}{% endblock %}
 * {% block content %}{% endblock %}
 * </pre>
 *
 * @see http://example.invalid/doc/templates.html#horizontal-reuse for details.
 */
class Twig_TokenParser_Use extends Twig_TokenParser
{
    public function parse(Twig_Token $token)
    {
        $template = $this->parser->getExpressionParser()->parseExpression();
        $stream = $this->parser->getStream();

        if (!$template instanceof Twig_Node_Expression_Constant) {
            throw new Twig_Error_Syntax('The template references in a "use" statement must be a string.', $stream->getCurrent()->getLine(), $stream->getFilename());
        }

        $targets = array();
        if ($stream->nextIf('with')) {
            do {
                $name = $stream->expect(Twig_Token:[REDACTED];

                $alias = $name;
                if ($stream->nextIf('as')) {
                    $alias = $stream->expect(Twig_Token:[REDACTED];
                }

                $targets[$name] = new Twig_Node_Expression_Constant($alias, -1);

                if (!$stream->nextIf(Twig_Token:[REDACTED], ',')) {
                    break;
                }
            } while (true);
        }

        $stream->expect(Twig_Token:[REDACTED];

        $this->parser->addTrait(new Twig_Node(array('template' => $template, 'targets' => new Twig_Node($targets))));
    }

    public function getTag()
    {
        return 'use';
    }
}
