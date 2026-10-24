<?php

/*
 * This file is part of Twig.
 *
 * (c) Fabien Potencier
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */
class Twig_Tests_LexerTest extends PHPUnit_Framework_TestCase
{
    public function testNameLabelForTag()
    {
        $template = '{% § %}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);

        $stream->expect(Twig_Token:[REDACTED];
        $this->assertSame('§', $stream->expect(Twig_Token:[REDACTED];
    }

    public function testNameLabelForFunction()
    {
        $template = '{{ §() }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);

        $stream->expect(Twig_Token:[REDACTED];
        $this->assertSame('§', $stream->expect(Twig_Token:[REDACTED];
    }

    public function testBracketsNesting()
    {
        $template = '{{ {"a":{"b":"c"}} }}';

        $this->assertEquals(2, $this->countToken($template, Twig_Token:[REDACTED], '{'));
        $this->assertEquals(2, $this->countToken($template, Twig_Token:[REDACTED], '}'));
    }

    protected function countToken($template, $type, $value = null)
    {
        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);

        $count = 0;
        while (!$stream->isEOF()) {
            $token = [REDACTED];
            if ($type === $token->getType()) {
                if (null === $value || $value === $token->getValue()) {
                    ++$count;
                }
            }
        }

        return $count;
    }

    public function testLineDirective()
    {
        $template = "foo\n"
            ."bar\n"
            ."{% line 10 %}\n"
            ."{{\n"
            ."baz\n"
            ."}}\n";

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);

        // foo\nbar\n
        $this->assertSame(1, $stream->expect(Twig_Token:[REDACTED];
        // \n (after {% line %})
        $this->assertSame(10, $stream->expect(Twig_Token:[REDACTED];
        // {{
        $this->assertSame(11, $stream->expect(Twig_Token:[REDACTED];
        // baz
        $this->assertSame(12, $stream->expect(Twig_Token:[REDACTED];
    }

    public function testLineDirectiveInline()
    {
        $template = "foo\n"
            ."bar{% line 10 %}{{\n"
            ."baz\n"
            ."}}\n";

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);

        // foo\nbar
        $this->assertSame(1, $stream->expect(Twig_Token:[REDACTED];
        // {{
        $this->assertSame(10, $stream->expect(Twig_Token:[REDACTED];
        // baz
        $this->assertSame(11, $stream->expect(Twig_Token:[REDACTED];
    }

    public function testLongComments()
    {
        $template = '{# '.str_repeat('*', 100000).' #}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);

        // should not throw an exception
    }

    public function testLongVerbatim()
    {
        $template = '{% verbatim %}'.str_repeat('*', 100000).'{% endverbatim %}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);

        // should not throw an exception
    }

    public function testLongVar()
    {
        $template = '{{ '.str_repeat('x', 100000).' }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);

        // should not throw an exception
    }

    public function testLongBlock()
    {
        $template = '{% '.str_repeat('x', 100000).' %}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);

        // should not throw an exception
    }

    public function testBigNumbers()
    {
        $template = '{{ 922337203685477580700 }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->next();
        $node = $stream->next();
        $this->assertEquals('922337203685477580700', $node->getValue());
    }

    public function testStringWithEscapedDelimiter()
    {
        $tests = array(
            "{{ 'foo \' bar' }}" => 'foo \' bar',
            '{{ "foo \" bar" }}' => 'foo " bar',
        );
        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        foreach ($tests as $template => $expected) {
            $stream = $lexer->tokenize($template);
            $stream->expect(Twig_Token:[REDACTED];
            $stream->expect(Twig_Token:[REDACTED], $expected);
        }
    }

    public function testStringWithInterpolation()
    {
        $template = 'foo {{ "bar #{ baz + 1 }" }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED], 'foo ');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar ');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'baz');
        $stream->expect(Twig_Token:[REDACTED], '+');
        $stream->expect(Twig_Token:[REDACTED], '1');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED];
    }

    public function testStringWithEscapedInterpolation()
    {
        $template = '{{ "bar \#{baz+1}" }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar #{baz+1}');
        $stream->expect(Twig_Token:[REDACTED];
    }

    public function testStringWithHash()
    {
        $template = '{{ "bar # baz" }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar # baz');
        $stream->expect(Twig_Token:[REDACTED];
    }

    /**
     * @expectedException Twig_Error_Syntax
     * @expectedExceptionMessage Unclosed """
     */
    public function testStringWithUnterminatedInterpolation()
    {
        $template = '{{ "bar #{x" }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);
    }

    public function testStringWithNestedInterpolations()
    {
        $template = '{{ "bar #{ "foo#{bar}" }" }}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar ');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'foo');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED];
    }

    public function testStringWithNestedInterpolationsInBlock()
    {
        $template = '{% foo "bar #{ "foo#{bar}" }" %}';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'foo');
        $stream->expect(Twig_Token:[REDACTED], 'bar ');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'foo');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 'bar');
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED];
    }

    public function testOperatorEndingWithALetterAtTheEndOfALine()
    {
        $template = "{{ 1 and\n0}}";

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $stream = $lexer->tokenize($template);
        $stream->expect(Twig_Token:[REDACTED];
        $stream->expect(Twig_Token:[REDACTED], 1);
        $stream->expect(Twig_Token:[REDACTED], 'and');
    }

    /**
     * @expectedException Twig_Error_Syntax
     * @expectedExceptionMessage Unclosed "variable" at line 3
     */
    public function testUnterminatedVariable()
    {
        $template = '

{{

bar


';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);
    }

    /**
     * @expectedException Twig_Error_Syntax
     * @expectedExceptionMessage Unclosed "block" at line 3
     */
    public function testUnterminatedBlock()
    {
        $template = '

{%

bar


';

        $lexer = new Twig_Lexer(new Twig_Environment($this->getMock('Twig_LoaderInterface')));
        $lexer->tokenize($template);
    }
}
