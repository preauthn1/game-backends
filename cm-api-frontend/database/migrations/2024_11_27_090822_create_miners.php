<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'miners';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci')->comment('礦機名稱');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('礦機圖示');
                $table->integer('currency_id')->default(0)->comment('挖礦幣種');
                $table->string('hash_rate', 20)->collation('utf8mb4_unicode_ci')->comment('哈希率TH/s');
                $table->string('power_consumption', 20)->collation('utf8mb4_unicode_ci')->comment('能量消化W');
                $table->decimal('day_revenue', 10, 4)->default(0.0000)->comment('每日收益');
                $table->tinyInteger('depreciation_cycle')->default(0)->comment('折舊週期(天)');
                $table->decimal('purchase_price', 10, 4)->default(0.0000)->comment('售價');
                $table->integer('purchase_total')->default(0)->comment('總購買數量');
                $table->decimal('rent_price', 10, 4)->default(0.0000)->comment('租賃價格');
                $table->integer('rent_total')->default(0)->comment('總租賃數量');
                $table->decimal('group_price', 10, 4)->default(0.0000);
                $table->integer('group_total')->default(0);
                $table->text('promo')->collation('utf8mb4_unicode_ci');
                $table->integer('sort')->default(0)->comment('排序');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->tinyInteger('is_show')->default(1)->comment('前端是否展示 0=隱藏; 1=展示;');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('礦機內容(JSON)');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'organization_id' => 1, 'name' => 'KS1R', 'image' => 'https://example.invalid/org/1/miner/6745a6c3ecbc7.jpeg', 'currency_id' => 102,
                    'hash_rate' => 3, 'power_consumption' => 5, 'day_revenue' => 2.0000, 'depreciation_cycle' => 14, 'purchase_price' => 500.0000,
                    'purchase_total' => 20, 'rent_price' => 300.0000, 'rent_total' => 0, 'group_price' => 250.0000, 'group_total' => 10,
                    'promo' => '{"purchase":{"num":2,"days":7},"group":{"num":1,"days":7}}', 'status' => 1, 'info' => '{"hash_rate":"3","power_consumption":"5","day_elec_fee":"1","custody_fee":"0.1","custody_fee_rate":"0.01","tag":"new","tag_color":"#cf1717"}'],
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
