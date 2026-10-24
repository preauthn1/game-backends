<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'transaction_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('wallet_type_id')->default(0)->comment('錢包系統編號');
                $table->integer('member_id')->comment('會員系統編號');
                $table->integer('transaction_type_id')->comment('交易類型系統編號(transaction_types.id)');
                $table->integer('order_id')->default(0)->comment('相關對應系統編號');
                $table->decimal('point', 30, 10)->default(0.0000000000)->comment('變更點數');
                $table->decimal('before_balance', 30, 10)->default(0.0000000000)->comment('變更前點數');
                $table->decimal('after_balance', 30, 10)->default(0.0000000000)->comment('變更後點數');
                $table->integer('operator')->default(1);
                $table->tinyInteger('status')->default(0)->comment('狀態編號');
                $table->string('memo', 500)->collation('utf8mb4_unicode_ci')->nullable()->default('');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('wallet_type_id', 'idx_' . $this->table_name . '_' . 'wallet_type_id');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('transaction_type_id', 'idx_' . $this->table_name . '_' . 'transaction_type_id');
                $table->index('order_id', 'idx_' . $this->table_name . '_' . 'order_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
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
