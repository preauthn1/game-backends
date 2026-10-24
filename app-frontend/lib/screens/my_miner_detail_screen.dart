import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../models/my_miner_detail.dart';
import '../providers/miner_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class MyMinerDetailScreen extends StatefulWidget {
  final int id;
  const MyMinerDetailScreen({super.key, required this.id});

  @override
  State<MyMinerDetailScreen> createState() => _MyMinerDetailScreenState();
}

class _MyMinerDetailScreenState extends State<MyMinerDetailScreen> {
  late Future<void> _future;
  MyMinerDetail? _detail;

  @override
  void initState() {
    super.initState();
    _future = _fetchData();
  }

  Future<void> _fetchData() async {
    await context.read<MinerProvider>().fetchMyMinerDetail(widget.id);
    setState(() {
      _detail = context.read<MinerProvider>().myMinerDetail;
    });
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: Text(_detail?.name ?? ''),
          backgroundColor: Colors.black,
          surfaceTintColor: Colors.transparent,
        ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            } else if (snapshot.hasError) {
              return const Center(
                child: Text('An error occurred'),
              );
            } else {
              if (_detail == null) {
                return const Center(child: Text('Miner not found'));
              }

              final data1 = [
                {
                  'label': 'Cumulative Output',
                  'value': '${_detail?.status?.totalRevenue ?? '-'} BTC',
                },
                {
                  'label': 'Electricity Fee Daily',
                  'value': '${formatNumber(_detail?.status?.dayElecFee)} USDT'
                },
                {
                  'label': 'Next Output',
                  'value': _detail?.status?.nextRevenueTime ?? '-'
                },
                {
                  'label': 'Custody fee',
                  'value': '${_detail?.status?.custodyFee ?? '-'} USDT/Unit/M'
                },
                {
                  'label': 'Custody fee percentage',
                  'value': '${_detail?.status?.custodyFeeRate}%/D'
                },
                {
                  'label': 'Depreciable Date',
                  'value': _detail?.status?.nextDepreciableTime ?? '-'
                },
              ];

              final data2 = [
                {
                  'label': 'Order Time',
                  'value': _detail?.details?.createTime ?? '-'
                },
                {
                  'label': 'Starting Time',
                  'value': _detail?.details?.startTime ?? '-'
                },
                {
                  'label': 'Miner Output',
                  'value': _detail?.details?.miningCurrency ?? '-'
                },
                {
                  'label': 'Hash Rate',
                  'value': '${_detail?.details?.hashRate ?? '-'} TH/s'
                },
                {
                  'label': 'Power Consumption',
                  'value': '${_detail?.details?.powerConsumption ?? '-'}W'
                },
                {
                  'label': 'Estimated revenue',
                  'value':
                      '${formatNumber(_detail?.details?.powerConsumption)} USDT/D'
                },
                {
                  'label': 'Depreciation cycle',
                  'value': _detail?.details?.depreciationCycle ?? '-'
                },
                {
                  'label': 'Reference Shutdown Price',
                  'value': '${formatNumber(_detail?.details?.balance)} USDT'
                },
              ];

              final data3 = _detail?.revenueList?.map((item) {
                    final value = item.revenue;
                    return {
                      'label': item.currencyName ?? '-',
                      'value': '${getSign(value)} ${formatNumber(value)}',
                      'date': item.summaryTime ?? '-',
                    };
                  }).toList() ??
                  [];

              return SingleChildScrollView(
                child: SafeArea(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Container(
                        height: 230,
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(vertical: 10),
                        color: Colors.black,
                        child: Padding(
                          padding: const EdgeInsets.only(bottom: 20),
                          child: Image.network(
                            _detail?.image ?? '',
                            width: double.infinity,
                          ),
                        ),
                      ),
                      Transform.translate(
                        offset: const Offset(0, -20),
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 20),
                          child: Column(
                            children: [
                              BaseCard(
                                width: double.infinity,
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'Status',
                                      style: TextStyle(
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                    ...data1.map(
                                      (item) => Column(
                                        children: [
                                          const SizedBox(height: 16),
                                          Row(
                                            mainAxisAlignment:
                                                MainAxisAlignment.spaceBetween,
                                            children: [
                                              Text(
                                                item['label']!,
                                                style: const TextStyle(
                                                  color: Color(0xB3FFFFFF),
                                                ),
                                              ),
                                              Text(item['value']!),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 20),
                              BaseCard(
                                width: double.infinity,
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'Details',
                                      style: TextStyle(
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                    ...data2.map(
                                      (item) => Column(
                                        children: [
                                          const SizedBox(height: 16),
                                          Row(
                                            mainAxisAlignment:
                                                MainAxisAlignment.spaceBetween,
                                            children: [
                                              Text(
                                                item['label']!,
                                                style: const TextStyle(
                                                  color: Color(0xB3FFFFFF),
                                                ),
                                              ),
                                              Text(item['value']!),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 20),
                              BaseCard(
                                width: double.infinity,
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'Output Records',
                                      style: TextStyle(
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                    ...data3.map(
                                      (item) => Column(
                                        children: [
                                          const SizedBox(height: 16),
                                          Row(
                                            mainAxisAlignment:
                                                MainAxisAlignment.spaceBetween,
                                            children: [
                                              Column(
                                                crossAxisAlignment:
                                                    CrossAxisAlignment.start,
                                                children: [
                                                  Text(item['label']!),
                                                  Text(
                                                    item['date']!,
                                                    style: const TextStyle(
                                                      color: Color(0xB3FFFFFF),
                                                    ),
                                                  ),
                                                ],
                                              ),
                                              Text(item['value']!),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              if (_detail?.sellBack?.isShow == "1")
                                Column(
                                  children: [
                                    const SizedBox(height: 24),
                                    BaseButton(
                                      text: 'Sell ​​back to the platform',
                                      onPressed:
                                          _detail?.sellBack?.isActive == "0"
                                              ? null
                                              : () => context.push(
                                                    '/my-miner/purchase/${_detail?.minerId}/${_detail?.mmId}/balance',
                                                  ),
                                    ),
                                  ],
                                ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              );
            }
          },
        ),
      ),
    );
  }
}
