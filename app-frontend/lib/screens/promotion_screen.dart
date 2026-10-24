import 'package:flutter/material.dart';
import 'package:miner/models/promotion.dart';
import 'package:provider/provider.dart';

import '../providers/promotion_provider.dart';
import '../utils/utils.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class PromotionScreen extends StatelessWidget {
  const PromotionScreen({super.key});

  void _clickPromotion(BuildContext context, Promotion promotion) {
    final type = promotion.type;
    if (type == '0') {
      // Open url
      final url = promotion.url;
      if (url != null) {
        launchURL(url);
      }
    } else if (type == '2') {
      // Show image
      Navigator.of(context).push(
        MaterialPageRoute(
          builder: (context) => LinearGradientBackground(
            child: Scaffold(
              appBar: AppBar(
                title: Text(promotion.title ?? ''),
              ),
              body: SingleChildScrollView(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  child: Image.network(
                    promotion.imageContent ?? '',
                    width: double.infinity,
                    fit: BoxFit.contain,
                  ),
                ),
              ),
            ),
          ),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Promotion'),
        ),
        body: FutureBuilder(
          future: context.read<PromotionProvider>().fetchPromotions(),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return const Center(child: Text('An error occurred'));
            }

            final promotions = context.read<PromotionProvider>().promotions;

            if (promotions.isEmpty) {
              return const Center(child: Text('No promotion available'));
            }

            return SafeArea(
              child: ListView.builder(
                padding: const EdgeInsets.all(20),
                itemCount: promotions.length,
                itemBuilder: (context, index) {
                  return BaseCard(
                    onTap: () {
                      _clickPromotion(context, promotions[index]);
                    },
                    margin: index == promotions.length - 1
                        ? null
                        : const EdgeInsets.only(bottom: 10),
                    padding: const EdgeInsets.all(8),
                    child: IntrinsicHeight(
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          SizedBox(
                            width: 120,
                            height: 87.6,
                            child: ClipRRect(
                              borderRadius: BorderRadius.circular(10.0),
                              child: Image.network(
                                promotions[index].image ?? '',
                                fit: BoxFit.cover,
                              ),
                            ),
                          ),
                          const SizedBox(width: 11),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  promotions[index].title ?? '-',
                                  style: const TextStyle(fontSize: 16),
                                ),
                                Text(
                                  promotions[index].content ?? '-',
                                  style: const TextStyle(
                                    fontSize: 14,
                                    color: Color(0xFFa3a2a4),
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(width: 8),
                          const Icon(Icons.arrow_forward_ios, size: 16),
                        ],
                      ),
                    ),
                  );
                },
              ),
            );
          },
        ),
      ),
    );
  }
}
