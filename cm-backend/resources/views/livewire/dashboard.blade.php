@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.dashboard.dashboard')}}</h1>
    </div>
    <div id="page-content">
        <div class="row">
            <div class="col-lg-11">
                <div class="row">
                    <div class="w-310">
                        <div class="panel panel-bordered-purple">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalReg')}}</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_registration']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-mint">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalDep')}}(USDT)</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_deposit_amount']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-success">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalDepNum')}}</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_deposit_number']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-danger">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalWd')}}(USDT)</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_withdrawal_amount']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-pink">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalWdNum')}}</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_withdrawal_number']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-warning">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.totalBalance')}}(USDT)</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['total_balance']}}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="w-310">
                        <div class="panel panel-bordered-purple">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.todayReg')}}</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['today_registration_number']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-mint">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.todayDepNum')}}</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['today_depositor_number']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-success">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.todayDep')}}(USDT)</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['today_deposit_amount']}}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="w-310">
                        <div class="panel panel-bordered-danger">
                            <div class="pad-all text-center">
                                <p>{{__('web_message.dashboard.todayWd')}}(USDT)</p>
                                <span class="text-3x text-thin">
                                    {{session()->get('pageCacheData')['today_withdrawal_amount']}}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="tab-base">
                        <ul class="nav nav-tabs">
                            <li class="active">
                                <a id="deposit7" data-toggle="tab" href="#tab-content">{{__('web_message.dashboard.last7dDepRep')}}</a>
                            </li>
                            <li>
                                <a id="deposit30" data-toggle="tab" href="#tab-content">{{__('web_message.dashboard.last30dDepRep')}}</a>
                            </li>
                            <li>
                                <a id="withdrawal7" data-toggle="tab" href="#tab-content">{{__('web_message.dashboard.last7dWdRep')}}</a>
                            </li>
                            <li>
                                <a id="withdrawal30" data-toggle="tab" href="#tab-content">{{__('web_message.dashboard.last30dWdRep')}}</a>
                            </li>
                        </ul>
                        <div class="tab-content">
                            <div id="tab-content" class="tab-pane fade active in">
                                <div style="width: 80%; margin: auto;">
                                    <canvas id="myChart"></canvas>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <style>
            canvas {
                width: 100% !important;
                height: 400px;
            }

            div.w-310 {
                width: 310px;
            }

            .tab-base .nav-tabs > li {
                width: 25%;
                text-align: center;
            }
        </style>
        @if(1==session()->get('pageCacheData')['showChart'])
            <script>
                $(function () {
                    $('#deposit7').click(function () {
                        doChart('deposit7');
                    });
                    $('#deposit30').click(function () {
                        doChart('deposit30');
                    });
                    $('#withdrawal7').click(function () {
                        doChart('withdrawal7');
                    });
                    $('#withdrawal30').click(function () {
                        doChart('withdrawal30');
                    });
                    // 获取 canvas 元素的上下文
                    var ctx = document.getElementById('myChart').getContext('2d');
                    // 创建图表
                    var myChart = new Chart(ctx, {
                        type: 'line', // 图表类型，例如 'line', 'bar', 'pie'
                        data: {
                            labels: @json(session()->get('pageCacheData')['deposit_list_7']['date']), // 从控制器传递的标签
                            datasets: [
                                {
                                    label: "{{__('web_message.dashboard.depositNum')}}", // 数据集标签
                                    data: @json(session()->get('pageCacheData')['deposit_list_7']['count']), // 从控制器传递的数据
                                    borderColor: '#f5426c', // 边框颜色
                                    backgroundColor: '#f5426c', // 背景颜色（用于填充）
                                    borderWidth: 2, // 边框宽度
                                    pointStyle: 'circle',
                                    pointRadius: 6,
                                    pointHoverRadius: 12,
                                    fill: false, // 是否填充
                                    yAxisID: 'y1', // 這裡指定右側 Y 軸
                                },
                                {
                                    label: "{{__('web_message.dashboard.depositAmount')}}(USDT)", // 数据集标签
                                    data: @json(session()->get('pageCacheData')['deposit_list_7']['amount']), // 从控制器传递的数据
                                    borderColor: '#42A5F5', // 边框颜色
                                    backgroundColor: '#42A5F5', // 背景颜色（用于填充）
                                    borderWidth: 2, // 边框宽度
                                    pointStyle: 'circle',
                                    pointRadius: 6,
                                    pointHoverRadius: 12,
                                    fill: false, // 是否填充
                                    yAxisID: 'y2', // 這裡指定左側 Y 軸
                                }
                            ]
                        },
                        options: {
                            responsive: true, // 图表是否响应式
                            maintainAspectRatio: false, // 是否保持宽高比
                            scales: {
                                y1: {
                                    type: 'linear',
                                    display: true,
                                    position: 'right',
                                    ticks: {
                                      color: '#f5426c',
                                      stepSize: 1
                                      
                                    },
                                },
                                y2: {
                                    type: 'linear',
                                    display: true,
                                    position: 'left',
                                    grid: {
                                        drawOnChartArea: false
                                    },
                                    ticks: {
                                        color: '#42A5F5'
                                    },
                                },
                            },
                        }
                    });

                    function doChart(index) {
                        switch (index) {
                            case 'deposit7':
                            default:
                                myChart.data.labels = @json(session()->get('pageCacheData')['deposit_list_7']['date']);
                                myChart.data.datasets[0].label = "{{__('web_message.dashboard.depositNum')}}";
                                myChart.data.datasets[0].data = @json(session()->get('pageCacheData')['deposit_list_7']['count']);
                                myChart.data.datasets[1].label = "{{__('web_message.dashboard.depositAmount')}}(USDT)";
                                myChart.data.datasets[1].data = @json(session()->get('pageCacheData')['deposit_list_7']['amount']);
                                break;
                            case 'deposit30':
                                myChart.data.labels = @json(session()->get('pageCacheData')['deposit_list_30']['date']);
                                myChart.data.datasets[0].label = "{{__('web_message.dashboard.depositNum')}}";
                                myChart.data.datasets[0].data = @json(session()->get('pageCacheData')['deposit_list_30']['count']);
                                myChart.data.datasets[1].label = "{{__('web_message.dashboard.depositAmount')}}(USDT)";
                                myChart.data.datasets[1].data = @json(session()->get('pageCacheData')['deposit_list_30']['amount']);
                                break;
                            case 'withdrawal7':
                                myChart.data.labels = @json(session()->get('pageCacheData')['withdrawal_list_7']['date']);
                                myChart.data.datasets[0].label = "{{__('web_message.dashboard.withdrawalNum')}}";
                                myChart.data.datasets[0].data = @json(session()->get('pageCacheData')['withdrawal_list_7']['count']);
                                myChart.data.datasets[1].label = "{{__('web_message.dashboard.withdrawalAmount')}}(USDT)";
                                myChart.data.datasets[1].data = @json(session()->get('pageCacheData')['withdrawal_list_7']['amount']);
                                break;
                            case 'withdrawal30':
                                myChart.data.labels = @json(session()->get('pageCacheData')['withdrawal_list_30']['date']);
                                myChart.data.datasets[0].label = "{{__('web_message.dashboard.withdrawalNum')}}";
                                myChart.data.datasets[0].data = @json(session()->get('pageCacheData')['withdrawal_list_30']['count']);
                                myChart.data.datasets[1].label = "{{__('web_message.dashboard.withdrawalAmount')}}(USDT)";
                                myChart.data.datasets[1].data = @json(session()->get('pageCacheData')['withdrawal_list_30']['amount']);
                                break;
                        }
                        //更新图表
                        myChart.update();
                    }
                });
            </script>
        @endif
    </div>
@endsection
