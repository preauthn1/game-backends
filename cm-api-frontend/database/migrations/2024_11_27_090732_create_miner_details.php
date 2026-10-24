<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'miner_details';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('miner_id')->default(0)->comment('礦機資訊編號(miners.id)');
                $table->tinyInteger('type')->default(1)->comment('類型: 0:rental, 1:purchase, 2:group');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('礦機資訊');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('miner_id', 'idx_' . $this->table_name . '_' . 'miner_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
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
