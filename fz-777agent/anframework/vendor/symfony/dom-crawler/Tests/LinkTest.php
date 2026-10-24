<?php

/*
 * This file is part of the Symfony package.
 *
 * (c) Fabien Potencier <user@example.invalid>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 */

namespace Symfony\Component\DomCrawler\Tests;

use Symfony\Component\DomCrawler\Link;

class LinkTest extends \PHPUnit_Framework_TestCase
{
    /**
     * @expectedException \LogicException
     */
    public function testConstructorWithANonATag()
    {
        $dom = new \DOMDocument();
        $dom->loadHTML('<html><div><div></html>');

        new Link($dom->getElementsByTagName('div')->item(0), 'http://example.invalid/');
    }

    /**
     * @expectedException \InvalidArgumentException
     */
    public function testConstructorWithAnInvalidCurrentUri()
    {
        $dom = new \DOMDocument();
        $dom->loadHTML('<html><a href="/foo">foo</a></html>');

        new Link($dom->getElementsByTagName('a')->item(0), 'example.invalid');
    }

    public function testGetNode()
    {
        $dom = new \DOMDocument();
        $dom->loadHTML('<html><a href="/foo">foo</a></html>');

        $node = $dom->getElementsByTagName('a')->item(0);
        $link = new Link($node, 'http://example.invalid/');

        $this->assertEquals($node, $link->getNode(), '->getNode() returns the node associated with the link');
    }

    public function testGetMethod()
    {
        $dom = new \DOMDocument();
        $dom->loadHTML('<html><a href="/foo">foo</a></html>');

        $node = $dom->getElementsByTagName('a')->item(0);
        $link = new Link($node, 'http://example.invalid/');

        $this->assertEquals('GET', $link->getMethod(), '->getMethod() returns the method of the link');

        $link = new Link($node, 'http://example.invalid/', 'post');
        $this->assertEquals('POST', $link->getMethod(), '->getMethod() returns the method of the link');
    }

    /**
     * @dataProvider getGetUriTests
     */
    public function testGetUri($url, $currentUri, $expected)
    {
        $dom = new \DOMDocument();
        $dom->loadHTML(sprintf('<html><a href="%s">foo</a></html>', $url));
        $link = new Link($dom->getElementsByTagName('a')->item(0), $currentUri);

        $this->assertEquals($expected, $link->getUri());
    }

    /**
     * @dataProvider getGetUriTests
     */
    public function testGetUriOnArea($url, $currentUri, $expected)
    {
        $dom = new \DOMDocument();
        $dom->loadHTML(sprintf('<html><map><area href="%s" /></map></html>', $url));
        $link = new Link($dom->getElementsByTagName('area')->item(0), $currentUri);

        $this->assertEquals($expected, $link->getUri());
    }

    /**
     * @dataProvider getGetUriTests
     */
    public function testGetUriOnLink($url, $currentUri, $expected)
    {
        $dom = new \DOMDocument();
        $dom->loadHTML(sprintf('<html><head><link href="%s" /></head></html>', $url));
        $link = new Link($dom->getElementsByTagName('link')->item(0), $currentUri);

        $this->assertEquals($expected, $link->getUri());
    }

    public function getGetUriTests()
    {
        return array(
            array('/foo', 'http://localhost/bar/foo/', 'http://localhost/foo'),
            array('/foo', 'http://localhost/bar/foo', 'http://localhost/foo'),
            array('
            /foo', 'http://localhost/bar/foo/', 'http://localhost/foo'),
            array('/foo
            ', 'http://localhost/bar/foo', 'http://localhost/foo'),

            array('foo', 'http://localhost/bar/foo/', 'http://localhost/bar/foo/foo'),
            array('foo', 'http://localhost/bar/foo', 'http://localhost/bar/foo'),

            array('', 'http://localhost/bar/', 'http://localhost/bar/'),
            array('#', 'http://localhost/bar/', 'http://localhost/bar/#'),
            array('#bar', 'http://localhost/bar?a=b', 'http://localhost/bar?a=b#bar'),
            array('#bar', 'http://localhost/bar/#foo', 'http://localhost/bar/#bar'),
            array('?a=b', 'http://localhost/bar#foo', 'http://localhost/bar?a=b'),
            array('?a=b', 'http://localhost/bar/', 'http://localhost/bar/?a=b'),

            array('http://example.invalid/foo', 'http://localhost/bar/', 'http://example.invalid/foo'),
            array('https://example.invalid/foo', 'https://localhost/bar/', 'https://example.invalid/foo'),
            array('mailto:user@example.invalid', 'http://localhost/foo', 'mailto:user@example.invalid'),

            // tests schema relative URL (issue #7169)
            array('//example.invalid/foo', 'http://localhost/bar/', 'http://example.invalid/foo'),
            array('//example.invalid/foo', 'https://localhost/bar/', 'https://example.invalid/foo'),

            array('?foo=2', 'http://localhost?foo=1', 'http://localhost?foo=2'),
            array('?foo=2', 'http://localhost/?foo=1', 'http://localhost/?foo=2'),
            array('?foo=2', 'http://localhost/bar?foo=1', 'http://localhost/bar?foo=2'),
            array('?foo=2', 'http://localhost/bar/?foo=1', 'http://localhost/bar/?foo=2'),
            array('?bar=2', 'http://localhost?foo=1', 'http://localhost?bar=2'),

            array('foo', 'http://example.invalid/bar/baz?/query/string', 'http://example.invalid/bar/foo'),

            array('.', 'http://localhost/foo/bar/baz', 'http://localhost/foo/bar/'),
            array('./', 'http://localhost/foo/bar/baz', 'http://localhost/foo/bar/'),
            array('./foo', 'http://localhost/foo/bar/baz', 'http://localhost/foo/bar/foo'),
            array('..', 'http://localhost/foo/bar/baz', 'http://localhost/foo/'),
            array('../', 'http://localhost/foo/bar/baz', 'http://localhost/foo/'),
            array('../foo', 'http://localhost/foo/bar/baz', 'http://localhost/foo/foo'),
            array('../..', 'http://localhost/foo/bar/baz', 'http://localhost/'),
            array('../../', 'http://localhost/foo/bar/baz', 'http://localhost/'),
            array('../../foo', 'http://localhost/foo/bar/baz', 'http://localhost/foo'),
            array('../../foo', 'http://localhost/bar/foo/', 'http://localhost/foo'),
            array('../bar/../../foo', 'http://localhost/bar/foo/', 'http://localhost/foo'),
            array('../bar/./../../foo', 'http://localhost/bar/foo/', 'http://localhost/foo'),
            array('../../', 'http://localhost/', 'http://localhost/'),
            array('../../', 'http://localhost', 'http://localhost/'),

            array('/foo', 'http://localhost?bar=1', 'http://localhost/foo'),
            array('/foo', 'http://localhost#bar', 'http://localhost/foo'),
            array('/foo', 'file:///', 'file:///foo'),
            array('/foo', 'file:///bar/baz', 'file:///foo'),
            array('foo', 'file:///', 'file:///foo'),
            array('foo', 'file:///bar/baz', 'file:///bar/foo'),
        );
    }
}
