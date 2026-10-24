@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.manager.operationLog')}}</h1>
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
                        <label>{{__('web_message.general.managerName')}}：</label>
                        <input type="text" class="form-control" wire:model="managerName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.apiPath')}}：</label>
                        <input type="text" class="form-control" wire:model="operationApiPath">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.apiName')}}：</label>
                        <input type="text" class="form-control" wire:model="operationApiName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="operationStatus">
                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.status.success')}}</option>
                            <option value="2">{{__('web_message.status.fail')}}</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.operationTime')}}：</label>
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
                <div class="table-responsive">
                    <table class="table txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>ID</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.managerName')}}</th>
                            <th>{{__('web_message.general.apiPath')}}</th>
                            <th>{{__('web_message.general.apiName')}}</th>
                            <th>{{__('web_message.general.request')}}</th>
                            <th>{{__('web_message.general.response')}}</th>
                            <th>{{__('web_message.general.costSec')}}</th>
                            <th>{{__('web_message.general.operationTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showDataPop({{$data->id}},'detail')">
                                        <i class="bi-list-task"></i>
                                    </button>
                                </td>
                                <td>{{ $data->id }}</td>
                                <td>{{ $data->statusTxt }}</td>
                                <td>{{ $data->username }}</td>
                                <td>{{ $data->api_path }}</td>
                                <td>{{ $data->api_name }}</td>
                                <td>{{ $data->request??'' }}</td>
                                <td>{{ $data->response_data??'' }}</td>
                                <td>{{ $data->time_cost??0 }}</td>
                                <td>{{ $data->created_at }}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="10">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="10" class="footable-visible">
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
        <!-- dataPop -->
        <div class="modal fade in" id="dataPop" role="dialog"
             style="display: {{'dataPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;margin: 15px 0;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.manager.operationDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body" style="padding-bottom: 10px;">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">ID</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.status')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popStatusTxt" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.managerName')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popManagerName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.apiPath')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popApiPath"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.apiName')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popApiName"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.request')}}</label>
                                    <div class="col-sm-9">
                                        <textarea rows="5" class="form-control" wire:model="popRequest"
                                                  disabled></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.response')}}</label>
                                    <div class="col-sm-9">
                                        <textarea rows="5" class="form-control" wire:model="popResponse"
                                                  disabled></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.costSec')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popCostSec" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">{{__('web_message.general.operationTime')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOperationTime" disabled>
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
                $('#popClose').click(function () {
                    $('#dataPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
