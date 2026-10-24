@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.member.manualOperation')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.general.type')}}：</label>
                        <select class="form-select form-control" wire:model="type">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                <option value="{{$data['id']}}">{{$data['type']}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.operator')}}：</label>
                        <input type="text" class="form-control" wire:model="operator">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="timeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" wire:click="showAddManualOperation()">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                        <div class="col-sm-6 table-toolbar-right">
                            <button class="btn btn-mint btn-export" wire:click="doExport">
                                <i class="bi-download"></i> {{__('web_message.general.export')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.general.type')}}</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.before')}}{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.after')}}{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.operator')}}</th>
                            <th>{{__('web_message.general.remark')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>{{$data->typeTxt}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{number_format($data->point??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->before_balance??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->after_balance??0,session()->get('defaultScale'))}}</td>
                                <td>{{$data->created_at}}</td>
                                <td>{{$data->operator}}</td>
                                <td style="white-space: pre-line;">{{$data->memo}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="8">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="8" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
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
        <!-- addManualOperation -->
        <div class="modal fade in" id="addManualOperation" role="dialog"
            style="display: {{'addManualOperation' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="addClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.member.addManualOperation')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addManualOperation">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.type')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addType" wire:change="changeType">
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                                <option value="{{$data['id']}}">{{$data['type']}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addUsername">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.amount')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addAmount"
                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-6">
                                        @if(!empty($memoOptions[$addType ?? '']))
                                            <select class="form-select form-control" wire:model="addMemo">
                                                <option value="">{{__('web_message.general.select')}}</option>
                                                @forelse(($memoOptions[$addType ?? '']) as $data)
                                                    <option value="{{$data}}">{{$data}}</option>
                                                @empty
                                                @endforelse
                                            </select>
                                        @else
                                            <textarea rows="3" class="form-control" 
                                                  wire:model="addMemo"></textarea>
                                        @endif
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
            });
        </script>
        <script>
            $(function () {
                $('#addClose').click(function () {
                    $('#addManualOperation').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
