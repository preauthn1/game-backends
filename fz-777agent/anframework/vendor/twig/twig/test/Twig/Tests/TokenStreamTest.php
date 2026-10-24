<?php

/*
 * This file is part of Twig.
 *
 * (c) Fabien Potencier
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

class Twig_Tests_TokenStreamTest extends PHPUnit_Framework_TestCase
{
    protected static $tokens;

    protected function setUp()
    {
        self::$tokens = array(
            new Twig_Token(Twig_Token:[REDACTED], 1, 1),
            new Twig_Token(Twig_Token:[REDACTED], 2, 1),
            new Twig_Token(Twig_Token:[REDACTED], 3, 1),
            new Twig_Token(Twig_Token:[REDACTED], 4, 1),
            new Twig_Token(Twig_Token:[REDACTED], 5, 1),
            new Twig_Token(Twig_Token:[REDACTED], 6, 1),
            new Twig_Token(Twig_Token:[REDACTED], 7, 1),
            new Twig_Token(Twig_Token:[REDACTED], 0, 1),
        );
    }

    public function testNext()
    {
        $stream = new Twig_TokenStream(self::$tokens);
        $repr = array();
        while (!$stream->isEOF()) {
            $token = [REDACTED];

            $repr[] = $token->getValue();
        }
        $this->assertEquals('1, 2, 3, 4, 5, 6, 7', implode(', ', $repr), '->next() advances the pointer and returns the current token');
    }

    /**
     * @expectedException Twig_Error_Syntax
     * @expectedMessage   Unexpected end of template
     */
    public function testEndOfTemplateNext()
    {
        $stream = new Twig_TokenStream(array(
            new Twig_Token(Twig_Token:[REDACTED], 1, 1),
        ));
        while (!$stream->isEOF()) {
            $stream->next();
        }
    }

    /**
     * @expectedException Twig_Error_Syntax
     * @expectedMessage   Unexpected end of template
     */
    public function testEndOfTemplateLook()
    {
        $stream = new Twig_TokenStream(array(
            new Twig_Token(Twig_Token:[REDACTED], 1, 1),
        ));
        while (!$stream->isEOF()) {
            $stream->look();
            $stream->next();
        }
    }
}
