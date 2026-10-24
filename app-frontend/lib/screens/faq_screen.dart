import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../utils/faq_list.dart';
import '../widgets/base_card.dart';
import '../widgets/linear_gradient_background.dart';

class FAQScreen extends StatelessWidget {
  const FAQScreen({super.key});

  Widget _buildItem(
    BuildContext context,
    String title,
    List<Map<String, dynamic>> list,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
        ),
        const SizedBox(height: 12),
        BaseCard(
          child: Column(
            children: list.asMap().entries.map(
              (entry) {
                int index = entry.key;
                var item = entry.value;
                BorderRadius? borderRadius;
                const circleRadius = Radius.circular(10);
                borderRadius = BorderRadius.only(
                  topLeft: index == 0 ? circleRadius : Radius.zero,
                  topRight: index == 0 ? circleRadius : Radius.zero,
                  bottomLeft:
                      index == list.length - 1 ? circleRadius : Radius.zero,
                  bottomRight:
                      index == list.length - 1 ? circleRadius : Radius.zero,
                );

                return ListTile(
                  shape: RoundedRectangleBorder(
                    borderRadius: borderRadius,
                  ),
                  title: Text(item['question']),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () {
                    context.push('/faq-detail/${item['id']}');
                  },
                );
              },
            ).toList(),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('FAQ'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Column(
            children: [
              ...faqList.map(
                (item) {
                  return Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      _buildItem(context, item['title'], item['items']),
                      const SizedBox(height: 20),
                    ],
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}
