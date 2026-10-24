<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'permissions';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci');
                $table->string('slug', 50)->collation('utf8mb4_unicode_ci');
                $table->string('http_method', 191)->collation('utf8mb4_unicode_ci')->nullable();
                $table->text('http_path')->collation('utf8mb4_unicode_ci');
                $table->timestamp('created_at')->nullable()
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')->nullable()
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique('name', 'uni_' . $this->table_name . '_' . 'name');
                $table->unique('slug', 'uni_' . $this->table_name . '_' . 'slug');
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
