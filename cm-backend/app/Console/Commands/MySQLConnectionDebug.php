<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;
use PDO;
use PDOException;

class MySQLConnectionDebug extends Command
{
    protected $signature = 'debug:mysql';
    protected $description = '診斷 MySQL 連線問題';

    public function handle()
    {
        $this->info('開始診斷 MySQL 連線...');

        // 1. 檢查 PHP PDO 模組
        if (!extension_loaded('pdo_mysql')) {
            $this->error('未安裝 PDO MySQL 擴充模組！');
            $this->info('請執行: sudo apt-get install php-mysql');
            return;
        }

        // 2. 檢查環境變數
        $this->info('檢查環境設定...');
        $dbConfig = [
            'host' => env('DB_HOST', '0.0.0.0'),
            'port' => env('DB_PORT', '3306'),
            'database' => env('DB_DATABASE', 'miner'),
            'username' => env('DB_USERNAME', 'developer'),
            'password' => env('DB_PASSWORD', '!QAZ2wsxForXSGames')
        ];

        // 檢查必要的配置
        foreach (['host', 'database', 'username', 'password'] as $key) {
            if (empty($dbConfig[$key])) {
                $this->error(".env 檔案中缺少 DB_{$key} 設定！");
                return;
            }
        }

        try {
            $pdo = new PDO("mysql:host={$dbConfig['host']};port={$dbConfig['port']};dbname={$dbConfig['database']}", $dbConfig['username'], $dbConfig['password']);
            $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
            $this->info('連線成功！');
        } catch (PDOException $e) {
            $this->error('連線失敗！');
            $this->error('錯誤訊息: ' . $e->getMessage());
            
            // 8. 提供常見解決方案
            $this->info('可能的解決方案:');
            $this->line('1. 確認 MySQL 服務是否正在運行');
            $this->line('   sudo systemctl status mysql');
            $this->line('2. 確認防火牆設定');
            $this->line('   sudo ufw status');
            $this->line('3. 檢查 MySQL 使用者主機限制');
            $this->line('4. 檢查 MySQL 連接埠是否正確');
            $this->line('   netstat -tlnp | grep mysql');
            $this->line('5. 確認 MySQL 8+ 的認證方式設定');
            $this->line('   ALTER USER \'username\'@\'hostname\' IDENTIFIED WITH mysql_native_password BY \'password\';');
            return;
        }

        // 9. 檢查最大連接數
        try {
            $maxConnections = $pdo->query('SHOW VARIABLES LIKE "max_connections"')->fetch(PDO::FETCH_ASSOC);
            $currentConnections = $pdo->query('SHOW STATUS LIKE "Threads_connected"')->fetch(PDO::FETCH_ASSOC);
            $this->info("最大連接數: {$maxConnections['Value']}");
            $this->info("當前連接數: {$currentConnections['Value']}");
        } catch (PDOException $e) {
            $this->warn('無法檢查連接數資訊');
        }

        $this->info('診斷完成！');
    }
} 