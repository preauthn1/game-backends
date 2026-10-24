<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'mining_blocks';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('currency_id')->default(0)->comment('貨幣資訊編號(currenies.id)');
                $table->integer('block_height')->default(0)->comment('區塊高度');
                $table->dateTime('block_time')->comment('區塊時間');
                $table->decimal('mining_difficulty', 10, 5)->default(0.00000)->comment('挖礦難度(Difficulty)');
                $table->decimal('mining_change', 5, 2)->default(0.00)->comment('挖礦難度增幅(change)');
                $table->decimal('block_reward', 10, 8)->default(0.00000000);
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('currency_id', 'idx_' . $this->table_name . '_' . 'currency_id');
                $table->index('block_time', 'idx_' . $this->table_name . '_' . 'block_time');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['currency_id' => 102, 'block_height' => 864864, 'block_time' => '2024-10-09 06:46:39', 'mining_difficulty' => 92.05000, 'mining_change' => 4.12, 'block_reward' => 3.12500000, 'status' => 1],
                ['currency_id' => 102, 'block_height' => 866880, 'block_time' => '2024-10-22 18:05:01', 'mining_difficulty' => 95.67000, 'mining_change' => 3.93, 'block_reward' => 3.12500000, 'status' => 1],
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
