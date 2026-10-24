@extends('livewire.template')

@php
    $isModify = (session()->get('pageCacheData')['popAction'] ?? '') == 'modify';
@endphp

@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{ __('web_message.menu.system.promotionSetting') }}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{ __('web_message.general.search') }}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{ __('web_message.general.type') }}：</label>
                        <select class="form-select form-control" wire:model="promotionType">
                            <option value="-1" selected>{{ __('web_message.general.select') }}</option>
                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                <option value="{{ $data->id }}">{{ $data->name }}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{ __('web_message.general.timeRange') }}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="promotionRange" autocomplete="off"
                                placeholder="{{ __('web_message.general.select') }} {{ __('web_message.general.timeRange') }}" />
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('today')">
                                {{ __('web_message.fastTime.today') }}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('yesterday')">
                                {{ __('web_message.fastTime.yesterday') }}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisWeek')">
                                {{ __('web_message.fastTime.thisWeek') }}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastWeek')">
                                {{ __('web_message.fastTime.lastWeek') }}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisMonth')">
                                {{ __('web_message.fastTime.thisMonth') }}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastMonth')">
                                {{ __('web_message.fastTime.lastMonth') }}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{ __('web_message.general.status') }}：</label>
                        <select class="form-select form-control" wire:model="promotionStatus">
                            <option value="-1" selected>{{ __('web_message.general.select') }}</option>
                            <option value="1">{{ __('web_message.general.statusList.enable') }}</option>
                            <option value="0">{{ __('web_message.general.statusList.disable') }}</option>

                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                        type="submit">{{ __('web_message.general.search') }}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addPromotion">
                                <i class="bi-plus-circle"></i> {{ __('web_message.general.add') }}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                            <tr>
                                <th></th>
                                <th>{{ __('web_message.general.status') }}</th>
                                <th>{{ __('web_message.general.type') }}</th>
                                <th>{{ __('web_message.general.title') }}</th>
                                <th>{{ __('web_message.general.content') }}</th>
                                <th>{{ __('web_message.menuMgmt.url') }}</th>
                                <th>{{ __('web_message.general.image') }}</th>
                                <th>{{ __('web_message.general.startTime') }}</th>
                                <th>{{ __('web_message.general.endTime') }}</th>
                            </tr>
                        </thead>
                        <tbody>
                            @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                                <tr>
                                    <td>
                                        <button class="btn btn-warning"
                                            wire:click="showDataPop({{ $data->promotion_id }},'modify')">
                                            <i class="bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-danger"
                                            wire:click="showDataPop({{ $data->promotion_id }},'detail')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    </td>
                                    <td style="font-size: 16px;"><i
                                            class="{{ 1 == $data->status ? 'bi-toggle-on' : 'bi-toggle-off' }}"></i></td>
                                    <td>{{ $data->typeTxt }}</td>
                                    <td>{{ $data->title }}</td>
                                    <td>
                                        <div>{{ $data->content }}</div>
                                    </td>
                                    <td>
                                        <a href="{{ $data->url }}" target="_blank">{{ $data->url }}</a>
                                    </td>
                                    <td>
                                        <img src="{{ $data->image }}"
                                            onclick="window.open('{{ $data->image }}','_blank');"
                                            style="max-height: 32px; width:60px; object-fit: contain; cursor: pointer;" />
                                    </td>
                                    <td>{{ $data->start_time }}</td>
                                    <td>{{ $data->end_time }}</td>
                                </tr>
                            @empty
                                <tr>
                                    <td colspan="9">{{ __('web_message.general.noData') }}</td>
                                </tr>
                            @endforelse
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan="9" class="footable-visible">
                                    <div class="text-right" style="position: relative;">
                                        <div class="total-entries">
                                            {{ session()->get('pageCacheData')['totalRow'] ?? 0 }}
                                            {{ __('web_message.general.entries') }}
                                        </div>
                                        <x-pagination></x-pagination>
                                    </div>
                                </td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <!-- addPromotion -->
        <div class="modal fade" id="addPromotion" role="dialog">
            <div class="modal-dialog" wire:ignore>
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" id="addPromotionClose">&times;</button>
                        <h4 class="modal-title">{{ __('web_message.menu.system.addPromotion') }}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addPromotion">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.type') }}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addPromotionType"
                                            id="addPromotionType">
                                            {{ __('web_message.general.type') }}</option>
                                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                                <option value="{{ $data->id }}">{{ $data->name }}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.title') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="addPromotionTitle">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.content') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <textarea rows="13" class="form-control" wire:model="addPromotionContent"></textarea>
                                    </div>
                                </div>
                                <div class="form-group" id="addUrlGroup">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.menuMgmt.url') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="addPromotionUrl"
                                            id="addPromotionUrl">
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.image') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" id="addPreviewImage"
                                                        style="width: 120px;" />
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" accept="image/*" id="addImage">
                                    </div>
                                </div>

                                <div class="form-group" id="addImageContentGroup">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.imageContent') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" id="addPreviewImageContent"
                                                        style="width: 120px;" />
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" accept="image/*"
                                            id="addImageContent">
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.startTime') }}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                            id="addPromotionStartTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.endTime') }}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                            id="addPromotionEndTime">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.status') }}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="add-promotion-status" class="toggle-switch" type="checkbox"
                                            wire:model="addPromotionStatus">
                                        <label for="add-promotion-status"></label>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                            type="submit">{{ __('web_message.general.add') }}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- modifyPromotion -->
        <div class="modal fade in" id="modifyPromotion" role="dialog"
            style="display: {{ 'modifyPromotion' == session('showPopId') ? 'block' : 'none' }};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyClose">&times;</button>
                        <h4 class="modal-title">
                            {{ $isModify ? __('web_message.menu.system.modifyPromotion') : __('web_message.menu.system.promotionDetail') }}
                        </h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyPromotionId">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.type') }}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="modifyPromotionType"
                                            id="modifyPromotionType" {{ $isModify ? '' : 'disabled' }}>
                                            <option value="0" selected>
                                                {{ __('web_message.general.select') }}{{ __('web_message.general.type') }}
                                            </option>
                                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                                <option value="{{ $data->id }}">{{ $data->name }}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.title') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="modifyPromotionTitle"
                                            {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.menuMgmt.order') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="modifyPromotionSort"
                                            oninput="value=value.replace(/\D/g,'')" {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.content') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <textarea rows="13" class="form-control" wire:model="modifyPromotionContent" {{ $isModify ? '' : 'disabled' }}></textarea>
                                    </div>
                                </div>
                                <div class="form-group" id="modifyUrlGroup"
                                    style="{{ $modifyPromotionType == 0 ? '' : 'display:none' }}">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.menuMgmt.url') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="modifyPromotionUrl"
                                            {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.image') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" id="modifyPreviewImage"
                                                        src="{{ $modifyImage }}"
                                                        onclick="window.open('{{ $modifyImage }}','_blank');"
                                                        style="width: 120px; cursor: pointer;" />
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" accept="image/*" id="modifyImage"
                                            {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>

                                <div class="form-group" id="modifyImageContentGroup"
                                    style="{{ $modifyPromotionType == 2 ? '' : 'display:none' }}">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{ __('web_message.general.imageContent') }}
                                    </label>
                                    <div class="col-sm-9">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" id="modifyPreviewImageContent"
                                                        onclick="window.open('{{ $modifyImageContent }}','_blank');"
                                                        src="{{ $modifyImageContent }}"
                                                        style="width: 120px; cursor: pointer;" />
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" accept="image/*"
                                            id="modifyImageContent" {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.startTime') }}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                            id="modifyPromotionStartTime" wire:model="promotionStartTime"
                                            {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.endTime') }}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" name="dates"
                                            id="modifyPromotionEndTime" wire:model="promotionEndTime"
                                            {{ $isModify ? '' : 'disabled' }}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{ __('web_message.general.status') }}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="modify-promotion-status" class="toggle-switch" type="checkbox"
                                            wire:model="modifyPromotionStatus" {{ $isModify ? '' : 'disabled' }}>
                                        <label for="modify-promotion-status"></label>
                                    </div>
                                </div>
                            </div>
                            @if ($isModify)
                                <div class="panel-footer">
                                    <div class="row">
                                        <div class="col-sm-offset-10">
                                            <button class="btn btn-mint"
                                                type="submit">{{ __('web_message.general.modify') }}</button>
                                        </div>
                                    </div>
                                </div>
                            @endif
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function() {
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function(ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format(
                        'YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('promotionRange', {
                        range: range
                    });
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function(ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('promotionRange', {
                        range: range
                    });
                });
                $('#modifyClose').click(function() {
                    $('#modifyPromotion').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                    $('#modifyImage').val('');
                    $('#modifyPreviewImage').attr('src', '');
                    $('#modifyImageContent').val('');
                    $('#modifyPreviewImageContent').attr('src', '');
                });
                $('input[name="dates"]').daterangepicker({
                    singleDatePicker: true,
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                    locale: {
                        format: 'YYYY-MM-DD HH:mm'
                    },
                    drops: 'up',
                });
                $('#addPromotionClose').click(function() {
                    $('#addImage').val('');
                    $('#addPreviewImage').attr('src', '');
                });
                $('#addPromotionStartTime').on('apply.daterangepicker', function(ev, picker) {
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('promotionStartTime', {
                        date: date
                    });
                });
                $('#addPromotionStartTime').on('cancel.daterangepicker', function(ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('promotionStartTime', {
                        date: date
                    });
                });
                $('#addPromotionStartTime').blur(function() {
                    Livewire.dispatch('promotionStartTime', {
                        date: $(this).val()
                    });
                });
                $('#addPromotionEndTime').on('apply.daterangepicker', function(ev, picker) {
                    console.log(ev.id);
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('promotionEndTime', {
                        date: date
                    });
                });
                $('#addPromotionEndTime').on('cancel.daterangepicker', function(ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('promotionEndTime', {
                        date: date
                    });
                });
                $('#addPromotionEndTime').blur(function() {
                    Livewire.dispatch('promotionEndTime', {
                        date: $(this).val()
                    });
                });
                $('#modifyPromotionStartTime').on('apply.daterangepicker', function(ev, picker) {
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('promotionStartTime', {
                        date: date
                    });
                });
                $('#modifyPromotionStartTime').on('cancel.daterangepicker', function(ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('promotionStartTime', {
                        date: date
                    });
                });
                $('#modifyPromotionStartTime').blur(function() {
                    Livewire.dispatch('promotionStartTime', {
                        date: $(this).val()
                    });
                });
                $('#modifyPromotionEndTime').on('apply.daterangepicker', function(ev, picker) {
                    console.log(ev.id);
                    let date = picker.startDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(date);
                    Livewire.dispatch('promotionEndTime', {
                        date: date
                    });
                });
                $('#modifyPromotionEndTime').on('cancel.daterangepicker', function(ev, picker) {
                    let date = '';
                    $(this).val(date);
                    Livewire.dispatch('promotionEndTime', {
                        date: date
                    });
                });
                $('#modifyPromotionEndTime').blur(function() {
                    Livewire.dispatch('promotionEndTime', {
                        date: $(this).val()
                    });
                });

                function handleFileRead(inputSelector, previewSelector, livewireEvent) {
                    document.querySelector(inputSelector).addEventListener("change", function() {
                        if (!this.files || !this.files[0]) return;
                        const FR = new FileReader();
                        FR.addEventListener("load", function(evt) {
                            document.querySelector(previewSelector).src = evt.target.result;
                            Livewire.dispatch(livewireEvent, {
                                data: evt.target.result
                            });
                        });
                        FR.readAsDataURL(this.files[0]);
                    });
                }

                handleFileRead("#addImage", "#addPreviewImage", "addImage");
                handleFileRead("#addImageContent", "#addPreviewImageContent", "addImageContent");
                handleFileRead("#modifyImage", "#modifyPreviewImage", "modifyImage");
                handleFileRead("#modifyImageContent", "#modifyPreviewImageContent", "modifyImageContent");

                $('#addPromotionType').change(function() {
                    updateAddModalUI();
                });

                $('#modifyPromotionType').change(function() {
                    updateModifyModalUI();
                });

                function updateAddModalUI() {
                    const type = $('#addPromotionType').val();
                    $('#addImageContentGroup').toggle(type == 2);
                    $('#addUrlGroup').toggle(type == 0);
                }

                function updateModifyModalUI() {
                    const type = $('#modifyPromotionType').val();
                    $('#modifyImageContentGroup').toggle(type == 2);
                    $('#modifyUrlGroup').toggle(type == 0);
                }

                updateAddModalUI();
            });
        </script>
    </div>
@endsection
