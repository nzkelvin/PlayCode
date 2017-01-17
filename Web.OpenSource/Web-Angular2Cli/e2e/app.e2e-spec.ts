import { WebAngular2CliPage } from './app.po';

describe('web-angular2-cli App', function() {
  let page: WebAngular2CliPage;

  beforeEach(() => {
    page = new WebAngular2CliPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
