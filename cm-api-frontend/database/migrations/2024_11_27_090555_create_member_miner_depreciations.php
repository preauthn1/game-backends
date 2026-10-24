<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miner_depreciations';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('member_miner_id')->default(0)->comment('會員礦機系統編號');
                $table->integer('mining_block_id')->default(0)->comment('挖礦難度系統編號');
                $table->decimal('residual_value', 30, 10)->comment('殘值');
                $table->decimal('depreciation_amount', 30, 10)->comment('折舊金額');
                $table->decimal('next_residual_value', 30, 10)->default(0.0000000000)->comment('下一次殘值');
                $table->decimal('next_depreciation_amount', 30, 10)->default(0.0000000000)->comment('下一次折舊金額');
                $table->text('info')->collation('utf8mb4_unicode_ci')->nullable()->comment('備註資訊');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('member_miner_id', 'idx_' . $this->table_name . '_' . 'member_miner_id');
                $table->index('mining_block_id', 'idx_' . $this->table_name . '_' . 'mining_block_id');
                $table->index('created_at', 'idx_' . $this->table_name . '_' . 'created_at');
            });
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
