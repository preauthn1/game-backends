<?php

namespace OSS\Tests;


use OSS\Model\RefererConfig;

class RefererConfigTest extends \PHPUnit_Framework_TestCase
{

    private $validXml = <<<BBBB
<?xml version="1.0" encoding="utf-8"?>
<RefererConfiguration>
<AllowEmptyReferer>true</AllowEmptyReferer>
<RefererList>
<Referer>http://example.invalid</Referer>
<Referer>https://example.invalid</Referer>
<Referer>http://www.*.com</Referer>
<Referer>https://www.?.example.invalid</Referer>
</RefererList>
</RefererConfiguration>
BBBB;

    private $validXml2 = <<<BBBB
<?xml version="1.0" encoding="utf-8"?>
<RefererConfiguration>
<AllowEmptyReferer>true</AllowEmptyReferer>
<RefererList>
<Referer>http://example.invalid</Referer>
</RefererList>
</RefererConfiguration>
BBBB;

    public function testParseValidXml()
    {
        $refererConfig = new RefererConfig();
        $refererConfig->parseFromXml($this->validXml);
        $this->assertEquals($this->cleanXml($this->validXml), $this->cleanXml($refererConfig->serializeToXml()));
    }

    public function testParseValidXml2()
    {
        $refererConfig = new RefererConfig();
        $refererConfig->parseFromXml($this->validXml2);
        $this->assertEquals(true, $refererConfig->isAllowEmptyReferer());
        $this->assertEquals(1, count($refererConfig->getRefererList()));
        $this->assertEquals($this->cleanXml($this->validXml2), $this->cleanXml(strval($refererConfig)));
    }

    private function cleanXml($xml)
    {
        return str_replace("\n", "", str_replace("\r", "", $xml));
    }
}
