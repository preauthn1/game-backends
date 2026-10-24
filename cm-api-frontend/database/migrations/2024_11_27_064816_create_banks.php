<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'banks';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('agent_group_id')->default(0)->comment('代理群組編號');
                $table->tinyInteger('type')->default(1)->comment('類型; 十位數:0:存款, 1:提款; 個位數:1:銀行號碼, 2:手機號碼, 3:加密貨幣, 4:外幣銀行');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci')->comment('銀行名稱');
                $table->string('code', 16)->collation('utf8mb4_unicode_ci')->comment('銀行代碼');
                $table->string('image', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('銀行icon');
                $table->integer('currency_id')->default(0)->comment('貨幣系統編號(currencies.id)');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('相關資訊(json)');
                $table->tinyInteger('tax_type')->default(0)->comment('手續費種類; 0:%數, 1:固定(暫定,還沒用到)');
                $table->decimal('tax', 11, 5)->default(0.00000)->comment('手續費; 1%=>0.01');
                $table->decimal('min_amount', 18, 4)->default(0.0000)->comment('最小金額');
                $table->decimal('max_amount', 18, 4)->default(0.0000)->comment('最大金額');
                $table->integer('sort')->default(0)->comment('排序');
                $table->tinyInteger('status')->comment('狀態');
                $table->string('remark', 45)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_group_id', 'idx_' . $this->table_name . '_' . 'agent_group_id');
                $table->index('type', 'idx_' . $this->table_name . '_' . 'type');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'agent_group_id' => 2, 'type' => 3, 'name' => 'USDT(TRC20)', 'code' => 'USDT-TRC20', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/USDT-TRC20.jpg', 'currency_id' => '101', 'tax_type' => 0, 'tax' => '0.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
                ['id' => 2, 'agent_group_id' => 2, 'type' => 3, 'name' => 'USDT(ERC20)', 'code' => 'USDT-ERC20', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/USDT-ERC20.jpg', 'currency_id' => '101', 'tax_type' => 0, 'tax' => '0.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
                ['id' => 3, 'agent_group_id' => 2, 'type' => 1, 'name' => 'MayBank', 'code' => 'MYD001', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/MayBank.jpg', 'currency_id' => '12', 'tax_type' => 0, 'tax' => '0.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
                ['id' => 4, 'agent_group_id' => 2, 'type' => 13, 'name' => 'USDT(TRC20)', 'code' => 'CCW01', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/USDT-TRC20.jpg', 'currency_id' => '101', 'tax_type' => 1, 'tax' => '1.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
                ['id' => 5, 'agent_group_id' => 2, 'type' => 13, 'name' => 'USDT(ERC20)', 'code' => 'CCW02', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/USDT-ERC20.jpg', 'currency_id' => '101', 'tax_type' => 1, 'tax' => '1.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
                ['id' => 6, 'agent_group_id' => 2, 'type' => 11, 'name' => 'MayBank', 'code' => 'MYW01', 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/bank/MayBank.jpg', 'currency_id' => '12', 'tax_type' => 1, 'tax' => '1.00000', 'min_amount' => '10.0000', 'max_amount' => '10000.0000', 'status' => 1],
            ]);
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
