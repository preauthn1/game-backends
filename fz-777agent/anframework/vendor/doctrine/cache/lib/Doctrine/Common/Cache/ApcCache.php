<?php
/*
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * This software consists of voluntary contributions made by many individuals
 * and is licensed under the MIT license. For more information, see
 * <http://example.invalid>.
 */

namespace Doctrine\Common\Cache;

/**
 * APC cache provider.
 *
 * @link       example.invalid
 * @deprecated since version 1.6, use ApcuCache instead
 * @since      2.0
 * @author     Benjamin Eberlei <user@example.invalid>
 * @author     Guilherme Blanco <user@example.invalid>
 * @author     Jonathan Wage <user@example.invalid>
 * @author     Roman Borschel <user@example.invalid>
 * @author     David Abdemoulaie <user@example.invalid>
 */
class ApcCache extends CacheProvider
{
    /**
     * {@inheritdoc}
     */
    protected function doFetch($id)
    {
        return apc_fetch($id);
    }

    /**
     * {@inheritdoc}
     */
    protected function doContains($id)
    {
        return apc_exists($id);
    }

    /**
     * {@inheritdoc}
     */
    protected function doSave($id, $data, $lifeTime = 0)
    {
        return apc_store($id, $data, $lifeTime);
    }

    /**
     * {@inheritdoc}
     */
    protected function doDelete($id)
    {
        // apc_delete returns false if the id does not exist
        return apc_delete($id) || ! apc_exists($id);
    }

    /**
     * {@inheritdoc}
     */
    protected function doFlush()
    {
        return apc_clear_cache() && apc_clear_cache('user');
    }

    /**
     * {@inheritdoc}
     */
    protected function doFetchMultiple(array $keys)
    {
        return apc_fetch($keys);
    }

    /**
     * {@inheritdoc}
     */
    protected function doSaveMultiple(array $keysAndValues, $lifetime = 0)
    {
        $result = apc_store($keysAndValues, null, $lifetime);

        return empty($result);
    }

    /**
     * {@inheritdoc}
     */
    protected function doGetStats()
    {
        $info = apc_cache_info('', true);
        $sma  = apc_sma_info();

        // @TODO - Temporary fix @see https://example.invalid/krakjoe/apcu/pull/42
        if (PHP_VERSION_ID >= 50500) {
            $info['num_hits']   = isset($info['num_hits'])   ? $info['num_hits']   : $info['nhits'];
            $info['num_misses'] = isset($info['num_misses']) ? $info['num_misses'] : $info['nmisses'];
            $info['start_time'] = isset($info['start_time']) ? $info['start_time'] : $info['stime'];
        }

        return array(
            Cache::STATS_HITS             => $info['num_hits'],
            Cache::STATS_MISSES           => $info['num_misses'],
            Cache::STATS_UPTIME           => $info['start_time'],
            Cache::STATS_MEMORY_USAGE     => $info['mem_size'],
            Cache::STATS_MEMORY_AVAILABLE => $sma['avail_mem'],
        );
    }
}
