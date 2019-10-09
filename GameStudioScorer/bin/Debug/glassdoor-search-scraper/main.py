'''
main.py
----------
Matthew Chatham
June 6, 2018
Given a company's landing page on Glassdoor and an output filename, scrape the
following information about each employee review:
Review date
Employee position
Employee location
Employee status (current/former)
Review title
Employee years at company
Number of helpful votes
Pros text
Cons text
Advice to mgmttext
Ratings for each of 5 categories
Overall rating
'''


import sys
import time
import pandas as pd
from argparse import ArgumentParser
import argparse
import logging
import logging.config
from selenium import webdriver as wd
import selenium
import numpy as np
from schema import SCHEMA
import json
import urllib
import datetime as dt

start = time.time()

DEFAULT_URL = ('https://www.glassdoor.com/Overview/Working-at-'
               'Premise-Data-Corporation-EI_IE952471.11,35.htm')

parser = ArgumentParser()
parser.add_argument('-n', '--name',
                    help='Name of the company.',
                    default=DEFAULT_URL)
parser.add_argument('--headless', action='store_true',
                    help='Run Chrome in headless mode.')
parser.add_argument('--username', help='Email address used to sign in to GD.')
parser.add_argument('-p', '--password', help='Password to sign in to GD.')
parser.add_argument('-c', '--credentials', help='Credentials file')
parser.add_argument('-b', '--browser', help='The location of the chrome executable if using a non-standard install location.')
args = parser.parse_args()

if args.credentials:
    with open(args.credentials) as f:
        d = json.loads(f.read())
        args.username = d['username']
        args.password = d['password']
else:
    try:
        with open('secret.json') as f:
            d = json.loads(f.read())
            args.username = d['username']
            args.password = d['password']
    except FileNotFoundError:
        msg = 'Please provide Glassdoor credentials.\
        Credentials can be provided as a secret.json file in the working\
        directory, or passed at the command line using the --username and\
        --password flags.'
        raise Exception(msg)


logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
ch = logging.StreamHandler()
ch.setLevel(logging.INFO)
logger.addHandler(ch)
formatter = logging.Formatter(
    '%(asctime)s %(levelname)s %(lineno)d\
    :%(filename)s(%(process)d) - %(message)s')
ch.setFormatter(formatter)

logging.getLogger('selenium').setLevel(logging.CRITICAL)
logging.getLogger('selenium').setLevel(logging.CRITICAL)


def sign_in():
    logger.info(f'Signing in to {args.username}')

    url = 'https://www.glassdoor.com/profile/login_input.htm'
    browser.get(url)

    # import pdb;pdb.set_trace()

    email_field = browser.find_element_by_name('username')
    password_field = browser.find_element_by_name('password')
    submit_btn = browser.find_element_by_xpath('//button[@type="submit"]')

    email_field.send_keys(args.username)
    password_field.send_keys(args.password)
    submit_btn.click()

    time.sleep(1)


def get_browser():
    logger.info('Configuring browser')
    chrome_options = wd.ChromeOptions()
    if args.browser:
        chrome_options.binary_location = args.browser
    if args.headless:
        chrome_options.add_argument('--headless')
    chrome_options.add_argument('log-level=3')
    browser = wd.Chrome(options=chrome_options)
    return browser


browser = get_browser()
idx = [0]
date_limit_reached = [False]


def extract_from_page():
    
    logger.info(f'Extracting links from page.')

    res = pd.DataFrame([], columns=SCHEMA)

    links = browser.find_elements_by_class_name('tightAll')
    logger.info(f'Found {len(links)} links on page.')

    for link in links:
        if(link.get_attribute('href') is not None):
            return link.get_attribute('href')
    
    return browser.current_url


def main():
    name = args.name.replace(" ", "-");
    logger.info(f'Creating url for {name}.');
    url = "https://www.glassdoor.com/Reviews/" + name + "-reviews-SRCH_KE0," + str(len(args.name)) + ".htm"

    res = pd.DataFrame([], columns=SCHEMA)

    
    
    sign_in()

    browser.get(url)
    logger.info(f'Accessing page.')
    time.sleep(1)

    link_url = extract_from_page()

    end = time.time()
    logger.info(f'Finished in {end - start} seconds')

    browser.quit()

    print(link_url);
    exit(0)


if __name__ == '__main__':
    main()
