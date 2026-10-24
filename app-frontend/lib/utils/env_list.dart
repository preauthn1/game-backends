import '../models/environment.dart';

final Map<String, Environment> envList = {
  'dev': Environment(
    name: 'dev',
    apiUrl: 'https://example.invalid',
    apiMd5Key: '&zSMvVTK4gbDvCcU',
    apiAesKey: 'M53B?xKnTVtVnjM^',
    apiIv: '3579653693526433',
    domain: 'example.invalid',
  ),
  'online': Environment(
    name: 'online',
    apiUrl: 'https://example.invalid',
    apiMd5Key: '&zSMvVTK4gbDvCcU',
    apiAesKey: 'M53B?xKnTVtVnjM^',
    apiIv: '3579653693526433',
    domain: 'example.invalid',
  ),
  'mybittest': Environment(
    name: 'online',
    apiUrl: 'https://example.invalid',
    apiMd5Key: '&zSMvVTK4gbDvCcU',
    apiAesKey: 'M53B?xKnTVtVnjM^',
    apiIv: '3579653693526433',
    domain: 'example.invalid',
  ),
};
