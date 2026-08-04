import Link from "next/link";
import { notFound } from "next/navigation";

import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale, localeLabels, locales } from "@/i18n/config";

export default async function LocaleHome({ params }: PageProps<"/[locale]">) {
  const { locale } = await params;

  if (!hasLocale(locale)) {
    notFound();
  }

  const dictionary = await getDictionary(locale);
  const topics = [
    dictionary.navigation.technology,
    dictionary.navigation.ai,
    dictionary.navigation.science,
    dictionary.navigation.software,
    dictionary.navigation.mobile,
  ];

  return (
    <div className="site-shell">
      <header className="site-header">
        <Link className="brand" href={`/${locale}`}>
          Peletnapechkai
        </Link>
        <nav aria-label="Language and region" className="locale-nav">
          {locales.map((supportedLocale) => (
            <Link
              aria-current={supportedLocale === locale ? "page" : undefined}
              className="locale-link"
              href={`/${supportedLocale}`}
              key={supportedLocale}
            >
              {localeLabels[supportedLocale]}
            </Link>
          ))}
        </nav>
      </header>

      <main>
        <section className="hero">
          <p className="eyebrow">{dictionary.home.eyebrow}</p>
          <h1>{dictionary.home.title}</h1>
          <p className="hero-description">{dictionary.home.description}</p>
          <div className="status-pill">
            <span aria-hidden="true" />
            {dictionary.home.status}
          </div>
        </section>

        <section className="content-grid" aria-labelledby="topics-title">
          <div>
            <p className="section-kicker">01</p>
            <h2 id="topics-title">{dictionary.home.topicsTitle}</h2>
            <ul className="topic-list">
              {topics.map((topic) => (
                <li key={topic}>{topic}</li>
              ))}
            </ul>
          </div>
          <aside className="principle-card">
            <p className="section-kicker">02</p>
            <h2>{dictionary.home.principleTitle}</h2>
            <p>{dictionary.home.principle}</p>
          </aside>
        </section>
      </main>

      <footer className="site-footer">
        <span>© {new Date().getUTCFullYear()} Peletnapechkai</span>
        <span>{localeLabels[locale]}</span>
      </footer>
    </div>
  );
}
