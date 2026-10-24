<?php

namespace OSS\Tests;

require_once __DIR__ . '/Common.php';

use OSS\Model\CnameConfig;

class BucketCnameTest extends \PHPUnit_Framework_TestCase
{
    private $bucketName;
    private $client;

    public function setUp()
    {
        $this->client = Common::getOssClient();
        $this->bucketName = 'php-sdk-test-bucket-' . strval(rand(0, 10000));
        $this->client->createBucket($this->bucketName);
    }

    public function tearDown()
    {
        $this->client->deleteBucket($this->bucketName);
    }

    public function testBucketWithoutCname()
    {
        $cnameConfig = $this->client->getBucketCname($this->bucketName);
        $this->assertEquals(0, count($cnameConfig->getCnames()));
    }

    public function testAddCname()
    {
        $this->client->addBucketCname($this->bucketName, 'example.invalid');
        $this->client->addBucketCname($this->bucketName, 'example.invalid');

        $ret = $this->client->getBucketCname($this->bucketName);
        $this->assertEquals(2, count($ret->getCnames()));

        // add another 2 cnames
        $this->client->addBucketCname($this->bucketName, 'example.invalid');
        $this->client->addBucketCname($this->bucketName, 'example.invalid');

        $ret = $this->client->getBucketCname($this->bucketName);
        $cnames = $ret->getCnames();
        $cnameList = array();

        foreach ($cnames as $c) {
            $cnameList[] = $c['Domain'];
        }
        $should = array(
            'example.invalid',
            'example.invalid',
            'example.invalid',
            'example.invalid'
        );
        $this->assertEquals(4, count($cnames));
        $this->assertEquals(sort($should), sort($cnameList));
    }

    public function testDeleteCname()
    {
        $this->client->addBucketCname($this->bucketName, 'example.invalid');
        $this->client->addBucketCname($this->bucketName, 'example.invalid');

        $ret = $this->client->getBucketCname($this->bucketName);
        $this->assertEquals(2, count($ret->getCnames()));

        // delete one cname
        $this->client->deleteBucketCname($this->bucketName, 'example.invalid');

        $ret = $this->client->getBucketCname($this->bucketName);
        $this->assertEquals(1, count($ret->getCnames()));
        $cnames = $ret->getCnames();
        $this->assertEquals('example.invalid', $cnames[0]['Domain']);
    }
}
